 namespace Minimart_Api.DTOS.Category
{
    public class CategoriesDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public int? ParentCategoryId { get; set; }
        public bool IsActive { get; set; } = true;
        public string  UserName { get; set; }

    }
}
