using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minimart_Api.Models
{
    /// <summary>
    /// Junction table that links merchants to their supported payment methods
    /// </summary>
    public class MerchantPaymentMethod
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public Guid MerchantId { get; set; }

        [Required]
        public int PaymentMethodId { get; set; }

        /// <summary>
        /// Merchant-specific configuration for this payment method (e.g., account details)
        /// </summary>
        [MaxLength(500)]
        public string? Configuration { get; set; }

        /// <summary>
        /// Whether this payment method is enabled for this merchant
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Date when this payment method was added to the merchant
        /// </summary>
        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date when this payment method was last updated for the merchant
        /// </summary>
        [Column(TypeName = "timestamp with time zone")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public virtual Merchants Merchant { get; set; } = null!;
        public virtual PaymentMethods PaymentMethod { get; set; } = null!;
    }
}