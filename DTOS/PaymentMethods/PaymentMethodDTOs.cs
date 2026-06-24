using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.DTOS.PaymentMethods
{
    #region System Payment Method DTOs

    /// <summary>
    /// DTO for system-wide payment method
    /// </summary>
    public class SystemPaymentMethodDto
    {
        public int PaymentMethodId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// DTO for creating a new system payment method
    /// </summary>
    public class CreateSystemPaymentMethodDto
    {
        [Required(ErrorMessage = "Payment method name is required")]
        [StringLength(100, ErrorMessage = "Payment method name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
        public string Description { get; set; } = string.Empty;

        [Url(ErrorMessage = "ImageUrl must be a valid URL")]
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    #endregion

    #region Merchant Payment Method DTOs

    /// <summary>
    /// DTO for merchant-specific payment method
    /// </summary>
    public class MerchantPaymentMethodDto
    {
        public int Id { get; set; }
        public Guid MerchantId { get; set; }
        public int PaymentMethodId { get; set; }
        public string PaymentMethodName { get; set; } = string.Empty;
        public string PaymentMethodDescription { get; set; } = string.Empty;
        //public string PaymentMethodImageUrl { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Configuration { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating/updating merchant payment method
    /// </summary>
    public class CreateMerchantPaymentMethodDto
    {
        [Required]
        public Guid MerchantId { get; set; }

        [Required]
        public int PaymentMethodId { get; set; }

        [StringLength(500, ErrorMessage = "Configuration cannot exceed 500 characters")]
        public string Configuration { get; set; } = string.Empty;

        public bool IsEnabled { get; set; } = true;
    }

    #endregion

    #region Response DTOs

    /// <summary>
    /// Response DTO for payment method operations
    /// </summary>
    public class PaymentMethodResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();

        public static PaymentMethodResponse<T> CreateSuccess(T data, string message = "Operation completed successfully")
        {
            return new PaymentMethodResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static PaymentMethodResponse<T> CreateError(string message, List<string>? errors = null)
        {
            return new PaymentMethodResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }

    #endregion
}