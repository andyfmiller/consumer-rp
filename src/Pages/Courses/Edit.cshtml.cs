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
    public class EditModel : PageModel
    {
        private readonly Data.ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EditModel(Data.ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Course = await _context.Course
                .Include(c => c.CourseInstructors)
                .ThenInclude(i => i.Instructor)
                .Include(c => c.CourseStudents)
                .ThenInclude(s => s.Student)
                .Include(c => c.CourseAssignments)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (Course == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (Course.CreatorId != user.Id)
            {
                return Unauthorized();
            }

            var instructorList = new SelectList(_context.Users.Where(u => u.CreatorId == user.Id), "Id", "UserName");
            foreach (var instructor in instructorList)
            {
                instructor.Selected = Course.CourseInstructors.Any(i => i.InstructorId == instructor.Value);
            }
            var studentList = new SelectList(_context.Users.Where(u => u.CreatorId == user.Id), "Id", "UserName");
            foreach (var student in studentList)
            {
                student.Selected = Course.CourseStudents.Any(i => i.StudentId == student.Value);
            }
            var assignmentList = new SelectList(_context.Assignment.Where(a => a.CreatorId == user.Id), "Id", "Name");
            foreach (var assignment in assignmentList)
            {
                assignment.Selected = Course.CourseAssignments.Any(a => a.AssignmentId.ToString() == assignment.Value);
            }

            ViewData["InstructorIds"] = instructorList;
            ViewData["StudentIds"] = studentList;
            ViewData["AssignmentIds"] = assignmentList;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var courseToUpdate = await _context.Course
                .Include(c => c.CourseInstructors)
                .ThenInclude(i => i.Instructor)
                .Include(c => c.CourseStudents)
                .ThenInclude(s => s.Student)
                .Include(c => c.CourseAssignments)
                .SingleOrDefaultAsync(m => m.Id == Course.Id);

            if (await TryUpdateModelAsync(courseToUpdate, "Course", c => c.Name))
            {
                foreach (var instructorId in InstructorIds)
                {
                    if (courseToUpdate.CourseInstructors.All(i => i.InstructorId != instructorId))
                    {
                        courseToUpdate.CourseInstructors.Add(new CourseInstructor
                        {
                            CourseId = courseToUpdate.Id,
                            InstructorId = instructorId
                        });
                    }
                }

                foreach (var instructor in courseToUpdate.CourseInstructors.ToList())
                {
                    if (InstructorIds.All(id => id != instructor.InstructorId))
                    {
                        courseToUpdate.CourseInstructors.Remove(instructor);
                    }
                }

                foreach (var studentId in StudentIds)
                {
                    if (courseToUpdate.CourseStudents.All(i => i.StudentId != studentId))
                    {
                        courseToUpdate.CourseStudents.Add(new CourseStudent
                        {
                            CourseId = courseToUpdate.Id,
                            StudentId = studentId
                        });
                    }
                }

                foreach (var student in courseToUpdate.CourseStudents.ToList())
                {
                    if (StudentIds.All(id => id != student.StudentId))
                    {
                        courseToUpdate.CourseStudents.Remove(student);
                    }
                }

                foreach (var assignmentId in AssignmentIds)
                {
                    if (courseToUpdate.CourseAssignments.All(ca => ca.AssignmentId != assignmentId))
                    {
                        var courseAssignment = new CourseAssignment
                        {
                            CourseId = courseToUpdate.Id,
                            AssignmentId = assignmentId
                        };
                        courseToUpdate.CourseAssignments.Add(courseAssignment);
                    }
                }

                foreach (var courseAssignment in courseToUpdate.CourseAssignments.ToList())
                {
                    if (AssignmentIds.All(id => id != courseAssignment.AssignmentId))
                    {
                        courseToUpdate.CourseAssignments.Remove(courseAssignment);
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(Course.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool CourseExists(int id)
        {
            return _context.Course.Any(e => e.Id == id);
        }
    }
}
