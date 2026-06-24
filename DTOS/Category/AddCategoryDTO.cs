namespace Minimart_Api.DTOS.Category
{
    public class AddCategoryDTO
    {
        public int CategoryID { get; set; }

        public string CategoryName { get; set; }

        public string ProductName { get; set; }

        public string Description { get; set; }

        public string[] SubCategoryName { get; set; }
    }
}
