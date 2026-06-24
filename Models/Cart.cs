using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.Models
{
    public class Cart
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CartId { get; set; }

        // Modern Identity support only
        public string? ApplicationUserId { get; set; }

        [MaxLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string? CartName { get; set; }

        [Column(TypeName = "timestamp with time zone")]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "timestamp with time zone")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation property to ApplicationUser (modern Identity system)
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser? User { get; set; }

        // Navigation property for cart items
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
