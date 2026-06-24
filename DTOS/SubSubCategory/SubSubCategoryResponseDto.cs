namespace Minimart_Api.DTOS.SubSubCategory
{
    public class SubSubCategoryResponseDto
    {
        public Guid SubSubCategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Slug { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public Guid SubCategoryId { get; set; }
        public Guid MerchantId { get; set; }
        public string? ImageUrl { get; set; }
        public int ProductCount { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
    }
}