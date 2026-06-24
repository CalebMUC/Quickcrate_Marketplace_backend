using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minimart_Api.Models
{
    /// <summary>
    /// Represents individual orders included in a payout
    /// </summary>
    public class PayoutTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid PayoutTransactionId { get; set; }

        /// <summary>
        /// Reference to the parent payout
        /// </summary>
        [Required]
        public Guid PayoutId { get; set; }

        /// <summary>
        /// Reference to the order included in this payout
        /// </summary>
        [Required]
        [StringLength(255)]
        public string OrderId { get; set; } = string.Empty;

        /// <summary>
        /// Total amount of the order
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OrderAmount { get; set; }

        /// <summary>
        /// Commission amount for this specific order
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CommissionAmount { get; set; }

        /// <summary>
        /// Net amount for this order after commission
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetAmount { get; set; }

        /// <summary>
        /// Date when the order was completed/delivered
        /// </summary>
        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTime OrderCompletedDate { get; set; }

        /// <summary>
        /// Date when this transaction was created
        /// </summary>
        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Customer name for reference
        /// </summary>
        [StringLength(255)]
        public string? CustomerName { get; set; }

        /// <summary>
        /// Order status at the time of payout creation
        /// </summary>
        [StringLength(50)]
        public string? OrderStatus { get; set; }

        /// <summary>
        /// Number of items in this order
        /// </summary>
        public int ItemCount { get; set; }

        /// <summary>
        /// Commission rate applied to this order
        /// </summary>
        [Column(TypeName = "decimal(5,4)")]
        public decimal CommissionRate { get; set; }

        // Navigation Properties
        public virtual Payout Payout { get; set; } = null!;
        public virtual Order Order { get; set; } = null!;
    }
}