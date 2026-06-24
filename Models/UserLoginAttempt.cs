using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.Models
{
    public class UserLoginAttempt
    {
        public long Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string IpAddress { get; set; } = string.Empty;

        public string? UserAgent { get; set; }

        public DateTime AttemptDate { get; set; } = DateTime.UtcNow;

        public bool Success { get; set; } = false;

        public string? FailureReason { get; set; }
    }
}