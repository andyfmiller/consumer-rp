using System;
using System.Linq;
using System.Threading.Tasks;
using Consumer.Models;
using LtiLibrary.AspNetCore.Extensions;
using LtiLibrary.AspNetCore.Membership;
using LtiLibrary.NetCore.Lis.v1;
using LtiLibrary.NetCore.Lis.v2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Consumer.Controllers
{
    [Route("[controller]/context/{contextId}")]
    public class MembershipController : MembershipControllerBase
    {
        private readonly Data.ApplicationDbContext _context;

        public MembershipController(Data.ApplicationDbContext context)
        {
            _context = context;
        }

        protected override Func<GetMembershipRequest, Task<GetMembershipResponse>> OnGetMembershipAsync => GetMembershipAsync;

        private async Task<GetMembershipResponse> GetMembershipAsync(GetMembershipRequest request)
        {
            #region Validate the request

            var assignmentId = Request.Query["assignmentId"].ToString();
            if (string.IsNullOrEmpty(assignmentId))
            {
                return BadRequest("AssignmentId is null or empty");
            }

            if (!int.TryParse(assignmentId, out var assignmentIntId))
            {
                return BadRequest($"{assignmentId} is not a valid assignmentId");
            }

            var assignment = await _context.Assignment
                .SingleOrDefaultAsync(a => a.Id == assignmentIntId);
            if (assignment == null)
            {
                return NotFound($"{nameof(Assignment)} not found");
            }

            var ltiRequest = await Request.ParseLtiRequestAsync();
            if (!assignment.ConsumerKey.Equals(ltiRequest.ConsumerKey))
            {
                return Unauthorized($"Unknown {nameof(ltiRequest.ConsumerKey)}");
            }

            var signature = ltiRequest.GenerateSignature(assignment.ConsumerSecret);
            if (!ltiRequest.Signature.Equals(signature))
            {
                return Unauthorized("Signatures do not match");
            }

            if (string.IsNullOrEmpty(request.ContextId))
            {
                return NotFound($"{nameof(request.ContextId)} is null or empty");
            }

            if (!int.TryParse(request.ContextId, out var courseId))
            {
                return NotFound($"{request.ContextId} is not a valid {nameof(request.ContextId)}");
            }

            var course = await _context.Course
                .Include(c => c.CourseInstructors)
                .ThenInclude(i => i.Instructor)
                .Include(c => c.CourseStudents)
                .ThenInclude(s => s.Student)
                .SingleOrDefaultAsync(c => c.Id == courseId);
            if (course == null)
            {
                return NotFound($"{nameof(Course)} not found");
            }

            #endregion

            var response = new GetMembershipResponse(Request, course.Id.ToString());

            foreach (var courseInstructor in course.CourseInstructors.ToList())
            {
                response.Membership.Add(new Membership
                {
                    Status = Status.Active,
                    Member = new Person
                    {
                        UserId = courseInstructor.Instructor.Id,
                        SourcedId = courseInstructor.Instructor.SourcedId,
                        Name = courseInstructor.Instructor.FullName,
                        FamilyName = courseInstructor.Instructor.FirstName,
                        GivenName = courseInstructor.Instructor.LastName,
                        Email = courseInstructor.Instructor.Email
                    },
                    Role = new[] {ContextRole.Instructor}
                });
            }

            foreach (var courseStudent in course.CourseStudents.ToList())
            {
                response.Membership.Add(new Membership
                {
                    Status = Status.Active,
                    Member = new Person
                    {
                        UserId = courseStudent.Student.Id,
                        SourcedId = courseStudent.Student.SourcedId,
                        Name = courseStudent.Student.FullName,
                        FamilyName = courseStudent.Student.FirstName,
                        GivenName = courseStudent.Student.LastName,
                        Email = courseStudent.Student.Email
                    },
                    Role = new[] {ContextRole.Learner}
                });
            }

            return response;
        }
    }
}
