using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Consumer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Consumer.Pages.Courses
{
    public class CreateModel : PageModel
    {
        private readonly Data.ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(Data.ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            ViewData["InstructorIds"] = new SelectList(_context.Users.Where(u => u.CreatorId == user.Id), "Id", "UserName");
            ViewData["StudentIds"] = new SelectList(_context.Users.Where(u => u.CreatorId == user.Id), "Id", "UserName");
            ViewData["AssignmentIds"] = new SelectList(_context.Assignment.Where(a => a.CreatorId == user.Id), "Id", "Name");

            return Page();
        }

        [BindProperty]
        public Course Course { get; set; }

        [BindProperty]
        [Display(Name = "Instructors")]
        public string[] InstructorIds { get; set; }

        [BindProperty]
        [Display(Name = "Students")]
        public string[] StudentIds { get; set; }

        [BindProperty]
        [Display(Name = "Assignments")]
        public int[] AssignmentIds { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var creator = await _userManager.GetUserAsync(User);

            var instructors = new List<ApplicationUser>();
            foreach (var instructorId in InstructorIds)
            {
                instructors.Add(await _userManager.FindByIdAsync(instructorId));
            }

            var students = new List<ApplicationUser>();
            foreach (var studentId in StudentIds)
            {
                students.Add(await _userManager.FindByIdAsync(studentId));
            }

            var course = new Course
            {
                Name = Course.Name,
                CreatorId = creator.Id
            };
            _context.Course.Add(course);

            foreach (var instructorId in InstructorIds)
            {
                course.CourseInstructors.Add(new CourseInstructor
                {
                    CourseId = course.Id,
                    InstructorId = instructorId
                });
            }

            foreach (var studentId in StudentIds)
            {
                course.CourseStudents.Add(new CourseStudent
                {
                    CourseId = course.Id,
                    StudentId = studentId
                });
            }

            foreach (var assignmentId in AssignmentIds)
            {
                course.CourseAssignments.Add(new CourseAssignment
                {
                    CourseId = course.Id,
                    AssignmentId = assignmentId
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}