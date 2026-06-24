using System.ComponentModel.DataAnnotations;
using Minimart_Api.DTOS.SubSubCategory;
using Minimart_Api.DTOS.Products;

namespace Minimart_Api.DTOS.SubCategory
{
    public class SubCategoryResponseDto
    {
        public Guid SubCategoryId { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }
        public string Slug { get; set; }

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        public Guid CategoryId { get; set; }
        
        /// <summary>
        /// Parent category name
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;

        public Guid MerchantID { get; set; }
        public string ImageUrl { get; set; }

        public int ProductCount { get; set; } = 0;

        /// <summary>
        /// List of products in this subcategory (only populated when includeProducts=true)
        /// </summary>
        public List<ProductSummaryDto> Products { get; set; } = new();

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedOn { get; set; }

        public string CreatedBy { get; set; }

        public string? UpdatedBy { get; set; }

        public List<SubSubCategoryResponseDto> SubSubCategories { get; set; } = new();
    }
}