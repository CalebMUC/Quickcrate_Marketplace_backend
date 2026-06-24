using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.DTOS.Features
{
    public class AddFeaturesDTO
    {
        public int FeatureID { get; set; }
        public string FeatureName { get; set; }

        public string FeatureOptions { get; set; } // JSON stored as text

        public int? CategoryID { get; set; }

        public int? SubCategoryID { get; set; }

        public int? SubSubCategoryID { get; set; }
        public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }
        public string SubSubCategoryName { get; set; }
    }
}
