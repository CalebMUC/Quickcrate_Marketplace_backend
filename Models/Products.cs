using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Minimart_Api.DTOS.Products;

namespace Minimart_Api.Models
{
    public class Product
    {
        [Key]
        public Guid ProductId { get; set; }

        [Required]
        [MaxLength(255)]
        public string ProductName { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string ProductDescription { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }

        [Required]
        public int StockQuantity { get; set; }

        [MaxLength(100)]
        public string SKU { get; set; } = string.Empty;

        // ============================================================
        // SEO PROPERTIES
        // ============================================================
        [StringLength(300)]
        [Column("Slug")]
        public string? Slug { get; set; }

        [Column("SlugUpdatedAt")]
        public DateTime? SlugUpdatedAt { get; set; }

        [StringLength(150)]
        [Column("MetaTitle")]
        public string? MetaTitle { get; set; }

        [StringLength(300)]
        [Column("MetaDescription")]
        public string? MetaDescription { get; set; }

        [StringLength(500)]
        [Column("MetaKeywords")]
        public string? MetaKeywords { get; set; }
        // ============================================================

        // Category Information
        public Guid CategoryId { get; set; }

        [MaxLength(255)]
        public string CategoryName { get; set; } = string.Empty;

        public Guid? SubCategoryId { get; set; }

        [MaxLength(255)]
        public string? SubCategoryName { get; set; }

        public Guid? SubSubCategoryId { get; set; }

        [MaxLength(255)]
        public string? SubSubCategoryName { get; set; }

        [MaxLength(4000)]
        public string ProductSpecification { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Features { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string BoxContents { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ProductType { get; set; } = string.Empty;

        // Status & Features
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;

        [MaxLength(50)]
        public string Status { get; set; } = "pending";

        // Images - stored as JSON string array
        [Column(TypeName = "text[]")]
        public List<string> ImageUrls { get; set; } = new();

        // Merchant Relationship
        public Guid MerchantID { get; set; }

        // Audit Fields
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(255)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime? UpdatedOn { get; set; }

        [MaxLength(255)]
        public string? UpdatedBy { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedOn { get; set; }

        [MaxLength(255)]
        public string? DeletedBy { get; set; }

        // Backward compatibility properties for legacy code
        [NotMapped]
        public string SearchKeyWord { get; set; } = string.Empty;

        [NotMapped]
        public bool InStock => IsActive && StockQuantity > 0;

        [NotMapped]
        public string KeyFeatures => Features;

        [NotMapped]
        public string Specification => ProductSpecification;

        [NotMapped]
        public string Box => BoxContents;

        [NotMapped]
        public string ImageUrl => ImageUrls?.FirstOrDefault() ?? "";

        [NotMapped]
        public string ImageType { get; set; } = string.Empty;

        // Navigation Properties
        [ForeignKey("MerchantID")]
        public virtual Merchants Merchant { get; set; } = null!;

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; } = null!;

        [ForeignKey("SubCategoryId")]
        public virtual SubCategory? SubCategory { get; set; }

        [ForeignKey("SubSubCategoryId")]
        public virtual SubSubCategory? SubSubCategory { get; set; }

        // Legacy collections
        public virtual ICollection<OrderTracking> OrderTrackings { get; set; } = new List<OrderTracking>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
        public virtual ICollection<Reviews> Reviews { get; set; } = new List<Reviews>();
    }
}