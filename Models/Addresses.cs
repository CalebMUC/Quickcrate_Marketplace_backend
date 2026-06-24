using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minimart_Api.Models
{
    public class Addresses
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AddressID { get; set; }

        // Modern Identity support only
        public string? ApplicationUserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(15)]
        public string Phonenumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string PostalAddress { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string PostalCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string County { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Town { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string ExtraInformation { get; set; } = string.Empty;

        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "timestamp with time zone")]
        public DateTime LastUpdatedOn { get; set; } = DateTime.UtcNow;

        public bool isDefault { get; set; } = false;

        // Navigation property to ApplicationUser (modern Identity system)
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
