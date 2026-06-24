using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.DTOS.Cart
{
    /// <summary>
    /// Request DTO for adding items to cart
    /// </summary>
    public class AddToCartDto
    {
        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        [Required]
        public Guid ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Request DTO for updating cart item quantity
    /// </summary>
    public class UpdateCartItemDto
    {
        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        [Required]
        public Guid ProductId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be 0 or greater")]
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Request DTO for removing items from cart
    /// </summary>
    public class RemoveFromCartDto
    {
        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        [Required]
        public Guid ProductId { get; set; }
    }

    /// <summary>
    /// Enhanced cart item response DTO
    /// </summary>
    public class CartItemDto
    {
        public int CartItemId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImage { get; set; }
        public decimal Price { get; set; }
        public decimal? Discount { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal => Price * Quantity;
        public decimal DiscountAmount => Discount.HasValue ? (Price * Quantity * Discount.Value / 100) : 0;
        public decimal Total => SubTotal - DiscountAmount;
        public bool InStock { get; set; }
        public int AvailableStock { get; set; }
        public Guid MerchantId { get; set; }
        public string? MerchantName { get; set; }
        public string? CategoryName { get; set; }
        public DateTime AddedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    /// <summary>
    /// Cart summary response DTO
    /// </summary>
    public class CartSummaryDto
    {
        public int CartId { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;
        public List<CartItemDto> Items { get; set; } = new();
        public int TotalItems => Items.Sum(x => x.Quantity);
        public decimal SubTotal => Items.Sum(x => x.SubTotal);
        public decimal TotalDiscount => Items.Sum(x => x.DiscountAmount);
        public decimal Total => Items.Sum(x => x.Total);
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Cart validation result
    /// </summary>
    public class CartValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<CartItemValidation> ItemValidations { get; set; } = new();
    }

    /// <summary>
    /// Individual cart item validation
    /// </summary>
    public class CartItemValidation
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public bool IsInStock { get; set; }
        public bool IsActive { get; set; }
        public int RequestedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public List<string> Issues { get; set; } = new();
    }

    /// <summary>
    /// Saved items DTOs
    /// </summary>
    public class SaveItemRequestDto
    {
        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        [Required]
        public Guid ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; } = 1;
    }

    public class SavedItemDto
    {
        public int Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImage { get; set; }
        public decimal Price { get; set; }
        public decimal? Discount { get; set; }
        public int Quantity { get; set; }
        public bool InStock { get; set; }
        public string? CategoryName { get; set; }
        public Guid MerchantId { get; set; }
        public string? MerchantName { get; set; }
        public DateTime SavedOn { get; set; }
    }

    /// <summary>
    /// Move to cart request DTO
    /// </summary>
    public class MoveToCartRequestDto
    {
        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        [Required]
        public Guid ProductId { get; set; }
    }

    /// <summary>
    /// Move to saved request DTO
    /// </summary>
    public class MoveToSavedRequestDto
    {
        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        [Required]
        public Guid ProductId { get; set; }
    }
}