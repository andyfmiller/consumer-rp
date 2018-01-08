using System.Linq;
using System.Threading.Tasks;
using Consumer.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Consumer.Pages.Courses
{
    public class DetailsModel : PageModel
    {
        private readonly Data.ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DetailsModel(Data.ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Course Course { get; set; }
        public string ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Course = await _context.Course
                .Include(c => c.CourseAssignments)
                .ThenInclude(a => a.Assignment)
                .Include(c => c.CourseInstructors)
                .ThenInclude(i => i.Instructor)
                .Include(c => c.CourseStudents)
                .ThenInclude(s => s.Student)
                .SingleOrDefaultAsync(c => c.Id == id);

            if (Course == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (User.IsInRole("Admin"))
            {
                if (Course.CreatorId != user.Id)
                {
                    return Unauthorized();
                }
            }
            else
            {
                if (Course.CourseStudents.All(s => s.StudentId != user.Id && Course.CourseInstructors.All(i => i.InstructorId != user.Id)))
                {
                    return Unauthorized();
                }
            }

            ReturnUrl = Request.GetDisplayUrl();

            return Page();
        }

        public async Task<string> GetScore(int assignmentId, string studentId)
        {
            var score = await _context.Score
                .SingleOrDefaultAsync(s =>
                    s.CourseId == Course.Id && s.AssignmentId == assignmentId && s.UserId == studentId);
            return score == null ? "n/a" : score.Value.ToString();
        }
    }
}
