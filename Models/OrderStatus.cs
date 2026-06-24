using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minimart_Api.Models
{
    /// <summary>
    /// OrderStatus model - simplified version for status management
    /// </summary>
    public class OrderStatus
    {
        [Key]
        public int StatusID { get; set; }

        [Required]
        [MaxLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string Status { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        [Column(TypeName = "varchar(500)")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string CreatedBy { get; set; } = string.Empty;

        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string UpdatedBy { get; set; } = string.Empty;

        [Column(TypeName = "timestamp with time zone")]
        public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;

    }
}