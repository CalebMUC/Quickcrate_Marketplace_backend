using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minimart_Api.Models
{
    public class OrderTracking
    {
        [Key]
        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string TrackingID { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string OrderID { get; set; } = string.Empty;

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public Guid MerchantID { get; set; }

        [Column(TypeName = "timestamp with time zone")]
        public DateTime TrackingDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "timestamp with time zone")]
        public DateTime ExpectedDeliveryDate { get; set; }

        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string? PreviousStatus { get; set; }

        [Required]
        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string CurrentStatus { get; set; } = "Processing";

        [MaxLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string? Carrier { get; set; }

        [MaxLength(500)]
        [Column(TypeName = "varchar(500)")]
        public string? TrackingNotes { get; set; }

        [MaxLength(255)]
        [Column(TypeName = "varchar(255)")]
        public string? Location { get; set; }

        // Audit fields
        [Required]
        [MaxLength(255)]
        [Column(TypeName = "varchar(255)")]
        public string CreatedBy { get; set; } = string.Empty;

        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [MaxLength(255)]
        [Column(TypeName = "varchar(255)")]
        public string? UpdatedBy { get; set; }

        [Column(TypeName = "timestamp with time zone")]
        public DateTime? UpdatedOn { get; set; }

        // Navigation properties - only modern relationships, no legacy OrderStatus references
        [ForeignKey("OrderID")]
        public virtual Order Order { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [ForeignKey("MerchantID")]
        public virtual Merchants Merchant { get; set; } = null!;
    }
}
