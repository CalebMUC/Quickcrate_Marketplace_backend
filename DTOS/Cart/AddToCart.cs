namespace Minimart_Api.DTOS.Cart
{
    public class AddToCart
    {
        public int Quantity { get; set; }
        public string? ProductID { get; set; }
        public string? ApplicationUserId { get; set; } // Identity User ID
    }
}
