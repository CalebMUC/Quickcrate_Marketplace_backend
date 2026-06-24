using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.Models
{
    public class BuyAgain
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }


        // Modern Identity support
        public string? ApplicationUserId { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTime PurchasedOn { get; set; } = DateTime.UtcNow;

        [Required]
        public int Quantity { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties - Updated to use ApplicationUser
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        // REMOVED: Legacy Users navigation property
        // [ForeignKey("UserId")]
        // public virtual Users User { get; set; }
    }
}
