using System.ComponentModel.DataAnnotations;

namespace EducationAssignmentPortal.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        [Required]
        public string Role { get; set; } = string.Empty;

        [Required]
        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string Type { get; set; } = "System";

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? TargetUrl { get; set; }
    }
}