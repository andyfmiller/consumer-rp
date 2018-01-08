namespace Consumer.Models
{
    public class CourseInstructor
    {
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public string InstructorId { get; set; }
        public ApplicationUser Instructor { get; set; }
    }
}
