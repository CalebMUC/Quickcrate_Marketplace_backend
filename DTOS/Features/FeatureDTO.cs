namespace Minimart_Api.DTOS.Features
{
    public class FeatureDTO
    {
        public string FeatureName { get; set; }
        public Dictionary<string, List<string>> FeatureOptions { get; set; }

        public Guid? CategoryId { get; set; }
        public Guid? SubCategoryId { get; set; }
        public Guid? SubSubCategoryId { get; set; }
    }
}
