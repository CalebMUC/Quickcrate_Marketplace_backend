namespace Minimart_Api.DTOS.Products
{
    public class AddProducts
    {
        // Legacy fields for backward compatibility (int-based)
        public int merchantID { get; set; }
        public int categoryId { get; set; }  // Legacy int-based category ID
        public int subCategoryId { get; set; }  // Legacy int-based subcategory ID
        public int subSubCategoryId { get; set; }  // Legacy int-based sub-subcategory ID

        // New fields for Merchant System (Guid-based)
        public Guid? MerchantGuid { get; set; }
        public Guid? CategoryGuid { get; set; }
        public Guid? SubCategoryGuid { get; set; }
        public Guid? SubSubCategoryGuid { get; set; }

        // Product Information
        public string productName { get; set; } = string.Empty;
        public string productID { get; set; } = string.Empty;
        public string searchKeyWord { get; set; } = string.Empty;
        public string categoryName { get; set; } = string.Empty;
        public string subCategoryName { get; set; } = string.Empty;
        public string subSubCategoryName { get; set; } = string.Empty;
        public string createdBy { get; set; } = string.Empty;
        public string productDetails { get; set; } = string.Empty;
        public string productSpecifications { get; set; } = string.Empty;
        public string productFeatures { get; set; } = string.Empty;
        public string boxContent { get; set; } = string.Empty;

        // Pricing and Stock
        public decimal price { get; set; }
        public int quantity { get; set; }
        public bool inStock { get; set; }
        public double discount { get; set; }

        // Images
        public string imageUrls { get; set; } = string.Empty;

        // New fields for Merchant System
        public string? SKU { get; set; }
        public string? Description { get; set; }
        public string? ProductType { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public string Status { get; set; } = "pending";
        public List<string>? ImageUrlsList { get; set; }
    }
}
