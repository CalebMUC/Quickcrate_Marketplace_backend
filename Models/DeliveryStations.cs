using Minimart_Api.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.Models
{
    public class DeliveryStations
    {
        [Key]
        public int DeliveryStationId { get; set; }

        [Required]
        [MaxLength(150)]
        [Column(TypeName = "varchar(150)")]
        public string DeliveryStationName { get; set; }

        [Required]
        [Column(TypeName = "timestamp")]
        public DateTime CreatedOn { get; set; }

        [Required]
        public int TownId { get; set; }

        [ForeignKey("TownId")]
        public virtual Towns Town { get; set; }
    }
}
