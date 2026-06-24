using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Minimart_Api.Models.Enums;

namespace Minimart_Api.Models
{
    public class Order
    {
        [Key]
        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string OrderID { get; set; } = string.Empty;

        // Modern Identity support only
        public string? ApplicationUserId { get; set; }
        //A foreign Key to OrderStatus
        [Required]
        public int StatusID { get; set; }


        [ForeignKey("StatusID")]
        public virtual OrderStatus? OrderStatus { get; set; }


        [Required]
        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string Status { get; set; } = "Pending";

        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTime OrderDate { get; set; }

        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTime DeliveryScheduleDate { get; set; }

        [MaxLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string? OrderedBy { get; set; }

        [Required]
        [Column(TypeName = "int")]
        public OrderStatusEnum StatusEnum { get; set; }

        [MaxLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string? StatusMessage { get; set; }

        [Required]
        public Guid PaymentID { get; set; }

        [Required]
        [MaxLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string PaymentConfirmation { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalOrderAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPaymentAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDeliveryFees { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalTax { get; set; }

        [Column(TypeName = "text")]
        public string? ShippingAddress { get; set; }

        [Required]
        [Column(TypeName = "jsonb")]
        public string ProductsJson { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column(TypeName = "varchar(255)")]
        public string PickupLocation { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "jsonb")]
        public string PaymentDetailsJson { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("PaymentID")]
        public virtual PaymentDetails? PaymentDetails { get; set; }

        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<OrderTracking> OrderTrackings { get; set; } = new List<OrderTracking>();
        public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
    }
}
