namespace Consumer.Models
{
    public class Score
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public int CourseId { get; set; }
        public string UserId { get; set; }
        public double Value { get; set; }
    }
}
