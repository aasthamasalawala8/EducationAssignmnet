namespace EducationAssignmentPortal.Models
{
    public class StudentAssignment
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public User Student { get; set; }

        public int AssignmentId { get; set; }
        public Assignment Assignment { get; set; }

        public string Status { get; set; } = "Pending";
        public DateTime? SubmittedDate { get; set; }
        public int? Marks { get; set; }
        public string? Remarks { get; set; }
    }
}