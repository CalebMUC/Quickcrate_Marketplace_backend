using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.Models
{
    public class Counties
    {
        [Key]
        public int CountyId { get; set; }

        [Required]
        [Column(TypeName = "int")]
        public int CountyCode { get; set; }

        [Required]
        [MaxLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string CountyName { get; set; }

        [Required]
        [Column(TypeName = "timestamp")]
        public DateTime CreatedOn { get; set; }

        // One County has many Towns
        public virtual ICollection<Towns> Towns { get; set; }
    }
}
