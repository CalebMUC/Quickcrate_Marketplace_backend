using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.Models
{
    public class ApplicationRole : IdentityRole
    {
        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    }
}