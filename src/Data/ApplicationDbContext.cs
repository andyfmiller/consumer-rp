using Consumer.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Consumer.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            builder.Entity<CourseAssignment>()
                .HasKey(ca => new {ca.CourseId, ca.AssignmentId});

            builder.Entity<CourseAssignment>()
                .HasOne(c => c.Course)
                .WithMany(ca => ca.CourseAssignments)
                .HasForeignKey(c => c.CourseId);

            builder.Entity<CourseAssignment>()
                .HasOne(a => a.Assignment)
                .WithMany(ca => ca.CourseAssignments)
                .HasForeignKey(a => a.AssignmentId);

            builder.Entity<CourseInstructor>()
                .HasKey(ci => new {ci.CourseId, ci.InstructorId});

            builder.Entity<CourseInstructor>()
                .HasOne(c => c.Course)
                .WithMany(ci => ci.CourseInstructors)
                .HasForeignKey(c => c.CourseId);

            builder.Entity<CourseInstructor>()
                .HasOne(c => c.Instructor)
                .WithMany(ci => ci.CourseInstructors)
                .HasForeignKey(c => c.InstructorId);

            builder.Entity<CourseStudent>()
                .HasKey(ci => new { ci.CourseId, ci.StudentId });

            builder.Entity<CourseStudent>()
                .HasOne(c => c.Course)
                .WithMany(ci => ci.CourseStudents)
                .HasForeignKey(c => c.CourseId);

            builder.Entity<CourseStudent>()
                .HasOne(c => c.Student)
                .WithMany(ci => ci.CourseStudents)
                .HasForeignKey(c => c.StudentId);
        }

        public DbSet<Course> Course { get; set; }
        public DbSet<Assignment> Assignment { get; set; }
        public DbSet<Score> Score { get; set; }
    }
}
