using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.Models
{
    /// <summary>
    /// Main Category model from the Merchant System
    /// </summary>
    public class Category
    {
        public Guid CategoryId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [MaxLength(255)]
        public string Slug { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        [Required]
        public Guid MerchantID { get; set; }

        public Guid? ParentId { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [MaxLength(255)]
        public string? MetaTitle { get; set; }

        [MaxLength(500)]
        public string? MetaDescription { get; set; }

        public int ProductCount { get; set; } = 0;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedOn { get; set; }

        [Required]
        [MaxLength(255)]
        public string CreatedBy { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? UpdatedBy { get; set; }

        // Navigation Properties
        public virtual Merchants Merchant { get; set; }
        public virtual Category Parent { get; set; }
        public virtual ICollection<Category> Children { get; set; } = new List<Category>();
        public virtual ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}