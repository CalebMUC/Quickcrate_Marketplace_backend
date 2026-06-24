namespace Minimart_Api.DTOS.Features
{
    public class FeatureRequestDTO
    {
        public Guid? CategoryID { get; set; }

        public Guid? SubCategoryID { get; set; }
        public Guid? SubSubCategoryID { get; set; }

    }
}
