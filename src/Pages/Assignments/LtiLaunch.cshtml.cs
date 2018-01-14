using System;
using System.Linq;
using System.Threading.Tasks;
using Consumer.Controllers;
using Consumer.Models;
using LtiLibrary.NetCore.Common;
using LtiLibrary.NetCore.Lis.v1;
using LtiLibrary.NetCore.Lti.v1;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Consumer.Pages.Assignments
{
    public class LtiLaunchModel : PageModel
    {
        private readonly Data.ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LtiLaunchModel(Data.ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public LtiRequest LtiRequest { get; set; }

        public async Task<IActionResult> OnGetAsync(int? courseId, int assignmentId, string returnUrl)
        {
            var assignment = await _context.Assignment
                .SingleOrDefaultAsync(m => m.Id == assignmentId);

            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            LtiRequest = new LtiRequest(LtiConstants.BasicLaunchLtiMessageType)
            {
                Url = new Uri(assignment.Url, UriKind.Absolute),
                ConsumerKey = assignment.ConsumerKey,
                LaunchPresentationDocumentTarget = DocumentTarget.iframe,
                LisPersonNameGiven = user.FirstName ?? string.Empty,
                LisPersonNameFamily = user.LastName ?? string.Empty,
                LisPersonEmailPrimary = user.Email ?? string.Empty,
                LisPersonSourcedId = user.SourcedId ?? string.Empty,
                ResourceLinkTitle = assignment.Name,
                ResourceLinkDescription = assignment.Description ?? string.Empty,
                ResourceLinkId = assignment.Id.ToString(),
                UserName = user.UserName
            };
            LtiRequest.SetCustomParameters(assignment.CustomParameters);

            if (!string.IsNullOrEmpty(returnUrl))
            {
                LtiRequest.LaunchPresentationReturnUrl = returnUrl;
            }

            if (courseId.HasValue)
            {
                var course = await _context.Course
                    .Include(c => c.CourseInstructors)
                    .Include(c => c.CourseStudents)
                    .SingleOrDefaultAsync(m => m.Id == courseId);
                if (course != null)
                {
                    LtiRequest.ContextId = course.Id.ToString();
                    LtiRequest.ContextTitle = course.Name;
                    LtiRequest.ContextType = ContextType.CourseSection;

                    if (course.CourseInstructors.Any(i => i.InstructorId == user.Id))
                    {
                        roles.Add(ContextRole.Instructor.ToString());
                    }

                    if (course.CourseStudents.Any(s => s.StudentId == user.Id))
                    {
                        roles.Add(ContextRole.Learner.ToString());
                    }

                    var resultSourcedId = new OutcomesController.ResultSourcedId
                    {
                        AssignmentId = assignment.Id,
                        CourseId = course.Id,
                        UserId = user.Id
                    };

                    if (Uri.TryCreate(new Uri(Request.GetDisplayUrl()), "/outcomes", out var serviceUri))
                    {
                        LtiRequest.LisResultSourcedId = JsonConvert.SerializeObject(resultSourcedId);
                        LtiRequest.LisOutcomeServiceUrl = serviceUri.AbsoluteUri;
                    }
                }
            }
            LtiRequest.Roles = string.Join(",", roles);

            LtiRequest.Signature = LtiRequest.SubstituteCustomVariablesAndGenerateSignature(assignment.ConsumerSecret);

            return Page();
        }
    }
}