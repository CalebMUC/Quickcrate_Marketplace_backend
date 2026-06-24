using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.Models
{
    public class RefreshToken
    {
        public long Id { get; set; }

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public string JwtId { get; set; } = string.Empty;

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        public DateTime CreationDate { get; set; } = DateTime.UtcNow;

        public DateTime ExpiryDate { get; set; }

        public bool Used { get; set; } = false;

        public bool Invalidated { get; set; } = false;

        public bool IsRevoked { get; set; } = false;

        public string? ReplacedByToken { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
