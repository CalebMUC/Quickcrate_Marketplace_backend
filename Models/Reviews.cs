using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minimart_Api.Models
{
    public class Reviews
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReviewId { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        // Modern Identity support only
        public string? ApplicationUserId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string? Title { get; set; }

        [Column(TypeName = "text")]
        public string? Comment { get; set; }

        [Column(TypeName = "timestamp with time zone")]
        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

        public bool IsVerifiedBuyer { get; set; } = false;

        public bool IsVisible { get; set; } = true;

        [MaxLength(1000)]
        [Column(TypeName = "varchar(1000)")]
        public string? AdminResponse { get; set; }

        // Navigation Properties - ApplicationUser only
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}