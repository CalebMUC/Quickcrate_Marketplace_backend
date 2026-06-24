using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Minimart_Api.Models;

public class PaymentDetails
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid PaymentID { get; set; }

    [Required]
    public int PaymentMethodID { get; set; }

    [Required]
    [MaxLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string TrxReference { get; set; }

    [MaxLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string PaymentReference { get; set; }

    [Required]
    [MaxLength(20)]
    [Column(TypeName = "varchar(20)")]
    public string Phonenumber { get; set; }

    [Required]
    [Column(TypeName = "money")]
    public decimal Amount { get; set; }

    [Required]
    [Column(TypeName = "timestamp with time zone")]
    public DateTime PaymentDate { get; set; }

    [MaxLength(20)]
    [Column(TypeName = "varchar(20)")]
    public string Status { get; set; } = "Pending";

    // ❌ REMOVE OrderID + FK to Order (this is what broke EF)

    [ForeignKey(nameof(PaymentMethodID))]
    public PaymentMethods PaymentMethod { get; set; }
}
