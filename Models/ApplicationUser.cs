using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minimart_Api.Models
{
    public class ApplicationUser : IdentityUser<string>
    {
        // -----------------------
        // Basic Profile Fields
        // -----------------------
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [MaxLength(100)]
        public string? DisplayName { get; set; }

        public bool? IsLoggedIn { get; set; }

        public DateTime? LastLogin { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? PasswordChangesOn { get; set; }

        public int? FailedAttempts { get; set; }

        public bool IsEmailVerified { get; set; }

        // Overridden by merchant model but kept for users
        public bool IsTemporaryPassword { get; set; } = false;


        // -----------------------
        // Merchant-Related Fields
        // -----------------------

        // Overrides/extends your existing CreatedAt logic 
        // Ensures backward compatibility
        public DateTime? LastPasswordReset { get; set; }

        public DateTime? LastLoginDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? TemporaryPasswordExpiry { get; set; }

        // -----------------------
        // Navigation Properties
        // -----------------------
        // One-to-one relationship with Merchants (if user is a merchant)
        public virtual Merchants? Merchant { get; set; }
        
        // One-to-many relationships
        public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Reviews> Reviews { get; set; } = new List<Reviews>();
        public virtual ICollection<Addresses> Addresses { get; set; } = new List<Addresses>();

        //public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
    

}