using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Minimart_Api.Models;

namespace Minimart_Api.DTOS.Products
{
    public class OrderItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderItemId { get; set; }

        [ForeignKey("Order")]
        [Column(TypeName = "varchar(50)")]  // Added explicit type for OrderId
        public string OrderId { get; set; }

        [ForeignKey("Product")]
        public Guid? ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }  // Removed explicit type as int maps directly

        [Required]
        [Column(TypeName = "numeric(18,2)")]  // Changed to PostgreSQL's numeric type
        public decimal Price { get; set; }

        // Navigation Properties
        public virtual Order? Order { get; set; }
        public virtual Product? Product { get; set; }
    }
}