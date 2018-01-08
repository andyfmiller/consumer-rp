namespace Consumer.Models
{
    public class CourseAssignment
    {
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public int AssignmentId { get; set; }
        public Assignment Assignment { get; set; }
    }
}