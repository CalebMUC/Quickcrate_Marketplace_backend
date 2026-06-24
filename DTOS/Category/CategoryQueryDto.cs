namespace Minimart_Api.DTOS.Category
{
    public class CategoryQueryDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public Guid? ParentId { get; set; }
        public string SortBy { get; set; } = "Name";
        public string SortOrder { get; set; } = "asc"; // asc or desc
    }
}