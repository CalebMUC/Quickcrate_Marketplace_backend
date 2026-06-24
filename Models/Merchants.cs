using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minimart_Api.Models
{
    /// <summary>
    /// Merchants model from the Merchant System
    /// </summary>
    public class Merchants
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid MerchantID { get; set; }


        public string? BusinessName { get; set; }

        public string? BusinessType { get; set; }

        public string? BusinessRegistrationNo { get; set; }

        public string? KRAPIN { get; set; }

        public string? BusinessNature { get; set; }

        public string? BusinessCategory { get; set; }

        public string? MerchantName { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public string? SocialMedia { get; set; }

        public bool? TermsAndCondition { get; set; }

        public string? DeliveryMethod { get; set; }

        public bool? ReturnPolicy { get; set; }

        public string? Status { get; set; } = "Active";

        /// <summary>
        /// Array of document URLs (e.g., business registration certificate, tax certificate, etc.)
        /// </summary>
        [Column(TypeName = "text[]")]
        public List<string> Documents { get; set; } = new List<string>();

        /// <summary>
        /// Date when the merchant registered on the platform
        /// </summary>
        [Column(TypeName = "timestamp with time zone")]
        public DateTime? RegistrationDate { get; set; } = DateTime.UtcNow;

        public string? ApplicationUserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        // Navigation Properties
        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
        public virtual ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
        public virtual ICollection<SubSubCategory> SubSubCategories { get; set; } = new List<SubSubCategory>();
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();

        public virtual ICollection<MerchantPaymentMethod> MerchantPaymentMethods { get; set; } = new List<MerchantPaymentMethod>();
        
        /// <summary>
        /// Navigation property to Orders - Orders that belong to this merchant
        /// </summary>
        //public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}