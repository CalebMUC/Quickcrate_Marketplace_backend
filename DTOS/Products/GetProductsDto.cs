using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.DTOS.Products
{
    public class GetProductsDto
    {
        public int MerchantID { get; set; } // Changed to int for SearchRepo compatibility

        public string? ProductName { get; set; }

        public string? Description { get; set; }

        public double Price { get; set; } // Changed to double for SearchRepo compatibility

        public int StockQuantity { get; set; }

        public int CategoryId { get; set; } // Changed to int for SearchRepo compatibility

        public string ProductId { get; set; } = null!; // Changed to string and removed extra semicolon

        public string ProductDescription { get; set; } = null!;
        
        public string CategoryName { get; set; } = null!; // Changed from null to null!

        public string ImageUrl { get; set; } = "";

        public bool InStock { get; set; }

        public double Discount { get; set; }

        public string SearchKeyWord { get; set; } = null!;

        public string KeyFeatures { get; set; } = null!;

        public string Specification { get; set; } = null!;

        public string Box { get; set; } = null!;

        public int SubCategoryId { get; set; } // Changed to int for compatibility

        public string? SubCategoryName { get; set; }

        public string? SubSubCategoryName { get; set; }

        public string? ProductType { get; set; }

        public DateTime? CreatedOn { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public string? UpdatedBy { get; set; }
    }
}
