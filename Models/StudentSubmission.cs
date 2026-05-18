using System.ComponentModel.DataAnnotations;

namespace EducationAssignmentPortal.Models
{
    public class StudentSubmission
    {
        public int Id { get; set; }

        public int AssignmentId { get; set; }

        public int StudentId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public string Comments { get; set; } = string.Empty;

        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        public int? Marks { get; set; }

        public string? Feedback { get; set; }
    }
}