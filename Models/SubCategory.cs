using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minimart_Api.Models
{
    /// <summary>
    /// SubCategory model from the Merchant System
    /// </summary>
    public class SubCategory
    {
        public Guid SubCategoryId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [MaxLength(255)]
        public string Slug { get; set; }

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        [Required]
        public Guid CategoryId { get; set; }

        [Required]
        public Guid MerchantID { get; set; }

        [MaxLength(500)]
        public string ImageUrl { get; set; }

        public int ProductCount { get; set; } = 0;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedOn { get; set; }

        [Required]
        [MaxLength(255)]
        public string CreatedBy { get; set; }

        [MaxLength(255)]
        public string? UpdatedBy { get; set; }

        // Navigation Properties
        public virtual Category Category { get; set; }
        public virtual Merchants Merchant { get; set; }
        public virtual ICollection<SubSubCategory> SubSubCategories { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}