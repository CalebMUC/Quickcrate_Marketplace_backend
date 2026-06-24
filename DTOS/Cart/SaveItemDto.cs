using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.DTOS.Cart
{
    public class SaveItemDto
    {
        public string ApplicationUserId { get; set; } = string.Empty; // Identity User ID
        public Guid ProductId { get; set; }
        public DateTime SavedOn { get; set; } = DateTime.UtcNow;
        public int Quantity { get; set; }
        public bool IsActive { get; set; }
    }
}
