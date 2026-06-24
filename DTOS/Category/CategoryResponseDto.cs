using Minimart_Api.DTOS.SubCategory;

namespace Minimart_Api.DTOS.Category
{
    public class CategoryResponseDto
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Slug { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public Guid MerchantId { get; set; }
        public Guid? ParentId { get; set; }
        public string? ImageUrl { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public int ProductCount { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
        public List<SubCategoryResponseDto> SubCategories { get; set; } = new();
    }
}