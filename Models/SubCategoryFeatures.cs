
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.Models
{
    public class SubCategoryFeatures
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubCategoryFeatureID { get; set; }

        [Required]
        public int SubCategoryId { get; set; }

        [Required]
        public int FeatureID { get; set; }

        // Navigation property to Subcategory (foreign key: SubCategoryId)
        //[ForeignKey("SubCategoryId")]
        //public virtual TSubcategoryid Subcategory { get; set; }

        // Navigation property to Feature (foreign key: FeatureID)
        [ForeignKey("FeatureID")]
        public virtual Features Features { get; set; }
    }
}
