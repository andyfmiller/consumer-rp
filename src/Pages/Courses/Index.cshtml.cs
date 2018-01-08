using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consumer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Consumer.Pages.Courses
{
    public class IndexModel : PageModel
    {
        private readonly Data.ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(Data.ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Course> Course { get;set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                Course = await _context.Course
                    .Include(c => c.CourseAssignments)
                    .ThenInclude(a => a.Assignment)
                    .Include(c => c.CourseInstructors)
                    .ThenInclude(i => i.Instructor)
                    .Include(c => c.CourseStudents)
                    .ThenInclude(s => s.Student)
                    .Where(c => c.CreatorId == user.Id)
                    .ToListAsync();
            }
            else
            {
                Course = await _context.Course
                    .Include(c => c.CourseAssignments)
                    .ThenInclude(a => a.Assignment)
                    .Include(c => c.CourseInstructors)
                    .ThenInclude(i => i.Instructor)
                    .Include(c => c.CourseStudents)
                    .ThenInclude(s => s.Student)
                    .Where(c => c.CourseStudents.Any(s => s.StudentId == user.Id) || c.CourseInstructors.Any(i => i.InstructorId == user.Id))
                    .ToListAsync();
            }
        }
    }
}
