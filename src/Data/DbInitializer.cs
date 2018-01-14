using System;
using System.Linq;
using System.Threading.Tasks;
using Consumer.Models;
using LtiLibrary.NetCore.Extensions;
using LtiLibrary.NetCore.Lis.v1;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Consumer.Data
{
    public static class DbInitializer
    {
        public static void Initialize(
            ApplicationDbContext context, 
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager
            )
        {
            CreateRolesAsync(roleManager).Wait();
            CreateUsersAsync(userManager).Wait();
            var course = CreateCoursesAsync(context, userManager).Result;
            CreateScoresAsync(context, course).Wait();
        }

        private static async Task CreateScoresAsync(ApplicationDbContext context, Course course)
        {
            foreach (var student in course.CourseStudents.ToList())
            {
                foreach (var assignment in course.CourseAssignments.ToList())
                {
                    var score = await context.Score.SingleOrDefaultAsync(s =>
                        s.CourseId == course.Id 
                        && s.AssignmentId == assignment.AssignmentId 
                        && s.UserId == student.StudentId);
                    if (score == null)
                    {
                        await context.Score.AddAsync(new Score
                        {
                            AssignmentId = assignment.AssignmentId,
                            CourseId = course.Id,
                            UserId = student.StudentId,
                            Value = Math.Round(new Random().NextDouble() * 100.0, MidpointRounding.AwayFromZero) / 100
                        });
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Seed Identity Roles with LTI Institutional Roles
        /// </summary>
        private static async Task CreateRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (Enum role in Enum.GetValues(typeof(InstitutionalRole)))
            {
                var name = role.GetUrn();
                if (!await roleManager.RoleExistsAsync(name))
                {
                    await roleManager.CreateAsync(new IdentityRole { Name = name });
                }
            }

            // Admin role is assigned to users that register. They are allowed
            // to create other users, courses, and assignments.
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Add SuperAdmin role to manage all data.
            if (!await roleManager.RoleExistsAsync("SuperAdmin"))
            {
                await roleManager.CreateAsync(new IdentityRole("SuperAdmin"));
            }
        }

        private static async Task CreateUsersAsync(UserManager<ApplicationUser> userManager)
        {
            var creator = await CreateUserAsync(userManager, "admin@contoso.edu", "Password1!", null, InstitutionalRole.Administrator);
            creator.CreatorId = creator.Id;
            await userManager.UpdateAsync(creator);
            await userManager.AddToRoleAsync(creator, "Admin");

            await CreateUserAsync(userManager, "instructor@contoso.edu", "Password1!", creator.Id, InstitutionalRole.Instructor);
            await CreateUserAsync(userManager, "student1@contoso.edu", "Password1!", creator.Id, InstitutionalRole.Student);
            await CreateUserAsync(userManager, "student2@contoso.edu", "Password1!", creator.Id, InstitutionalRole.Student);
            await CreateUserAsync(userManager, "student3@contoso.edu", "Password1!", creator.Id, InstitutionalRole.Student);
            await CreateUserAsync(userManager, "student4@contoso.edu", "Password1!", creator.Id, InstitutionalRole.Student);
            await CreateUserAsync(userManager, "student5@contoso.edu", "Password1!", creator.Id, InstitutionalRole.Student);
            await CreateUserAsync(userManager, "student6@contoso.edu", "Password1!", creator.Id, InstitutionalRole.Student);
            await CreateUserAsync(userManager, "student7@contoso.edu", "Password1!", creator.Id, InstitutionalRole.Student);
            await CreateUserAsync(userManager, "student8@contoso.edu", "Password1!", creator.Id, InstitutionalRole.Student);
            await CreateUserAsync(userManager, "student9@contoso.edu", "Password1!", creator.Id, InstitutionalRole.Student);
        }

        private static async Task<Course> CreateCoursesAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            var creator = await userManager.FindByNameAsync("admin@contoso.edu");

            var assignment = await context.Assignment.FirstOrDefaultAsync(a => a.CreatorId == creator.Id);
            if (assignment == null)
            {
                assignment = new Assignment
                {
                    Name = "LTI Test",
                    Description = "This is an LTI test tool.",
                    ConsumerKey = "12345",
                    ConsumerSecret = "secret",
                    CreatorId = creator.Id,
                    Url = "https://lti.tools/saltire/tp"
                };
                await context.Assignment.AddAsync(assignment);
                await context.SaveChangesAsync();
            }

            var course = await context.Course
                .Include(c => c.CourseAssignments)
                .ThenInclude(c => c.Assignment)
                .Include(c => c.CourseInstructors)
                .ThenInclude(c => c.Instructor)
                .Include(c => c.CourseStudents)
                .ThenInclude(c => c.Student)
                .FirstOrDefaultAsync(c => c.CreatorId == creator.Id);
            if (course == null)
            {
                course = new Course
                {
                    Name = "Sample Course",
                    CreatorId = creator.Id
                };
                await context.Course.AddAsync(course);
                await context.SaveChangesAsync();
            }

            if (course.CourseAssignments.All(a => a.AssignmentId != assignment.Id))
            {
                course.CourseAssignments.Add(new CourseAssignment
                {
                    CourseId = course.Id,
                    AssignmentId = assignment.Id
                });
            }


            foreach (var user in await userManager.Users.Where(u => u.CreatorId == creator.Id).ToListAsync())
            {
                if (await userManager.IsInRoleAsync(user, InstitutionalRole.Instructor.GetUrn()))
                {
                    if (course.CourseInstructors.All(i => i.InstructorId != user.Id))
                    {
                        course.CourseInstructors.Add(new CourseInstructor
                        {
                            CourseId = course.Id,
                            InstructorId = user.Id
                        });
                    }
                }
                else if (await userManager.IsInRoleAsync(user, InstitutionalRole.Student.GetUrn()))
                {
                    if (course.CourseStudents.All(s => s.StudentId != user.Id))
                    {
                        course.CourseStudents.Add(new CourseStudent
                        {
                            CourseId = course.Id,
                            StudentId = user.Id
                        });
                    }
                }
            }

            await context.SaveChangesAsync();

            return course;
        }

        private static async Task<ApplicationUser> CreateUserAsync(UserManager<ApplicationUser> userManager, string username, string password, string creatorId, InstitutionalRole role)
        {
            var user = await userManager.FindByNameAsync(username);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = username
                };
                await userManager.CreateAsync(user, password);
            }
            user.CreatorId = creatorId;
            user.Email = username;
            user.FirstName = role.ToString();
            user.LastName = "User";
            await userManager.UpdateAsync(user);
            await userManager.AddToRoleAsync(user, role.GetUrn());

            return user;
        }
    }
}
