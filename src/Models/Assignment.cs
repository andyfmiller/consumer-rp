using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Consumer.Models
{
    public class Assignment
    {
        public int Id { get; set; }

        [Display(Name = "Key")]
        [StringLength(50)]
        public string ConsumerKey { get; set; }

        [Display(Name = "Secret")]
        [StringLength(50)]
        public string ConsumerSecret { get; set; }

        [Display(Name = "Custom parameters")]
        [StringLength(1024)]
        public string CustomParameters { get; set; }

        public string Description { get; set; }

        [Display(Name = "LTI Link")]
        public bool IsLtiLink
        {
            get
            {
                return !string.IsNullOrEmpty(ConsumerKey) && !string.IsNullOrEmpty(ConsumerSecret);
            }
        }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Display(Name = "URL")]
        [StringLength(1024)]
        public string Url { get; set; }

        public string CreatorId { get; set; }

        [Display(Name = "Courses")]
        public ICollection<CourseAssignment> CourseAssignments { get; set; }
    }
}
