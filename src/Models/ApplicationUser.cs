using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Consumer.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "First name")]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Display(Name = "Last name")]
        [StringLength(50)]
        public string LastName { get; set; }

        [Display(Name = "Full name")]
        public string FullName
        {
            get
            {
                var fullname = string.Format("{0} {1}", FirstName, LastName).Trim();
                return string.IsNullOrEmpty(fullname) ? "[Anonymous User]" : fullname;
            }
        }

        [Display(Name = "Send email address")]
        public bool SendEmail { get; set; }

        [Display(Name = "Send names")]
        public bool SendNames { get; set; }

        [Display(Name = "SIS ID")]
        [StringLength(50)]
        public string SourcedId { get; set; }

        public string CreatorId { get; set; }

        [Display(Name = "Courses")]
        public ICollection<CourseInstructor> CourseInstructors { get; set; }

        [Display(Name = "Courses")]
        public ICollection<CourseStudent> CourseStudents { get; set; }

    }
}
