using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minimart_Api.Models
{
    /// <summary>
    /// Main payout aggregate entity representing a payout period per merchant
    /// </summary>
    public class Payout
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid PayoutId { get; set; }

        /// <summary>
        /// Reference to the merchant receiving this payout
        /// </summary>
        [Required]
        public Guid MerchantId { get; set; }

        /// <summary>
        /// Total gross amount from all orders in this payout period
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal GrossAmount { get; set; }

        /// <summary>
        /// Commission amount deducted from gross amount
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CommissionAmount { get; set; }

        /// <summary>
        /// Commission rate applied (e.g., 0.05 for 5%)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(5,4)")]
        public decimal CommissionRate { get; set; }

        /// <summary>
        /// Net amount after commission deduction (Gross - Commission)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetAmount { get; set; }

        /// <summary>
        /// Current status of the payout
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = PayoutStatus.Pending;

        /// <summary>
        /// Start date of the payout period
        /// </summary>
        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTime PeriodStartDate { get; set; }

        /// <summary>
        /// End date of the payout period
        /// </summary>
        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTime PeriodEndDate { get; set; }

        /// <summary>
        /// Date when payout was created
        /// </summary>
        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date when payout is scheduled for processing
        /// </summary>
        [Column(TypeName = "timestamp with time zone")]
        public DateTime? ScheduledDate { get; set; }

        /// <summary>
        /// Date when payout was completed
        /// </summary>
        [Column(TypeName = "timestamp with time zone")]
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// Reference to the payment method used for this payout
        /// </summary>
        public int? PaymentMethodId { get; set; }

        /// <summary>
        /// Number of orders included in this payout
        /// </summary>
        [Required]
        public int OrderCount { get; set; }

        /// <summary>
        /// Number of unique products sold in this payout period
        /// </summary>
        [Required]
        public int ProductCount { get; set; }

        /// <summary>
        /// Optional notes or comments about the payout
        /// </summary>
        [StringLength(1000)]
        public string? Notes { get; set; }

        /// <summary>
        /// Failure reason if payout failed
        /// </summary>
        [StringLength(500)]
        public string? FailureReason { get; set; }

        /// <summary>
        /// External payment reference (from payment provider)
        /// </summary>
        [StringLength(255)]
        public string? ExternalPaymentReference { get; set; }

        /// <summary>
        /// User who created this payout (admin/system)
        /// </summary>
        [StringLength(450)]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// User who last updated this payout
        /// </summary>
        [StringLength(450)]
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Last update timestamp
        /// </summary>
        [Column(TypeName = "timestamp with time zone")]
        public DateTime? UpdatedDate { get; set; }

        // Navigation Properties
        public virtual Merchants Merchant { get; set; } = null!;
        public virtual PaymentMethods? PaymentMethod { get; set; }
        public virtual ICollection<PayoutTransaction> PayoutTransactions { get; set; } = new List<PayoutTransaction>();
    }

    /// <summary>
    /// Payout status constants
    /// </summary>
    public static class PayoutStatus
    {
        public const string Pending = "Pending";
        public const string Scheduled = "Scheduled";
        public const string Processing = "Processing";
        public const string Completed = "Completed";
        public const string Failed = "Failed";
        public const string Cancelled = "Cancelled";
    }
}