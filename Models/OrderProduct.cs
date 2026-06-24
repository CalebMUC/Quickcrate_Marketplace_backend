using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Minimart_Api.Models.Enums;
using StackExchange.Redis;

namespace Minimart_Api.Models
{
    public class OrderProduct
    {
        [Key]
        public int OrderProductID { get; set; }

        // Foreign key to Order
        [Required]
        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string OrderID { get; set; } = string.Empty;

        // Foreign key to Product
        [Required]
        public Guid ProductId { get; set; }

        // Foreign key to Merchant
        [Required]
        public Guid MerchantID { get; set; }

        [Required]
        [Column(TypeName = "int")]
        public int Quantity { get; set; }

        public decimal TotalPrice { get; set; }

        [Required]
        public OrderStatusEnum Status { get; set; } = OrderStatusEnum.Pending;


        // Audit fields
        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        [Column(TypeName = "timestamp with time zone")]
        public DateTime? UpdatedOn { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("OrderID")]
        public virtual Order Order { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [ForeignKey("MerchantID")]
        public virtual Merchants Merchant { get; set; } = null!;
    }
}
