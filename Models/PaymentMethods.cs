using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.Models
{
    public class PaymentMethods
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentMethodID { get; set; }

        [Required]
        [MaxLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; }

        [MaxLength(255)]
        [Column(TypeName = "varchar(255)")]
        public string Description { get; set; } // Optional description of the payment method

        [Required]
        public bool IsActive { get; set; } = true; // To track if the payment method is active

        public string ImageUrl { get; set; } // URL to an image/icon representing the payment method
         
        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreatedDate { get; set; } = DateTime.Now; // Creation date

        [Column(TypeName = "timestamp with time zone")]
        public DateTime UpdatedOn { get; set; } // Creation date

        // Navigation property for related payment details
        public ICollection<PaymentDetails> PaymentDetails { get; set; }

        public virtual ICollection<MerchantPaymentMethod> MerchantPaymentMethods { get; set; } = new List<MerchantPaymentMethod>();
    }
}
