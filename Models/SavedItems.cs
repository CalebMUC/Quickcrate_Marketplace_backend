using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.Models
{
    public class SavedItems
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTime SavedOn { get; set; } = DateTime.UtcNow;

        // Modern Identity support only
        public string? ApplicationUserId { get; set; }

        // Navigation Properties
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
