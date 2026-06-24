namespace Minimart_Api.DTOS.Cart
{
    public class CartResults
    {
        public Guid productID { get; set; }
        public Guid MerchantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;

        public string ProductDescription { get; set; } = string.Empty;
        public string KeyFeatures { get; set; }
        public string Specification { get; set; } = string.Empty;
        public string Box { get; set; } = string.Empty;
        public int Quantity { get; set; }

        public decimal? price { get; set; }

        public bool InStock { get; set; }

        public int CartID { get; set; }

        public int CartItemID { get; set; }
    }
}
