using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.Models
{
    public class Features
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FeatureID { get; set; }

        [Required]
        [MaxLength(255)]
        [Column(TypeName = "varchar(255)")]
        public string FeatureName { get; set; }

        [Column(TypeName = "jsonb")]  // Changed to jsonb for better JSON handling in PostgreSQL
        public string FeatureOptions { get; set; }

        [ForeignKey("Category")]
        public Guid? CategoryID { get; set; }

        [ForeignKey("SubCategory")]
        public Guid? SubCategoryID { get; set; }

        [ForeignKey("SubSubCategory")]
        public Guid? SubSubCategoryID { get; set; }

        // Navigation properties - Updated to use new Category system
        public virtual Category Category { get; set; }
        public virtual SubCategory SubCategory { get; set; }
        public virtual SubSubCategory SubSubCategory { get; set; }
    }
}