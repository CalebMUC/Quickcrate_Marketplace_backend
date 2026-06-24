using Minimart_Api.DTOS.PaymentMethods;

namespace Minimart_Api.Services.PaymentMethods
{
    /// <summary>
    /// Service interface for managing payment methods (both system and merchant-specific)
    /// </summary>
    public interface IPaymentMethodService
    {
        #region System Payment Methods

        /// <summary>
        /// Get all system payment methods
        /// </summary>
        Task<PaymentMethodResponse<IEnumerable<SystemPaymentMethodDto>>> GetSystemPaymentMethodsAsync();

        /// <summary>
        /// Get a specific system payment method by ID
        /// </summary>
        Task<PaymentMethodResponse<SystemPaymentMethodDto>> GetSystemPaymentMethodByIdAsync(int id);

        /// <summary>
        /// Create a new system payment method (Admin only)
        /// </summary>
        Task<PaymentMethodResponse<SystemPaymentMethodDto>> CreateSystemPaymentMethodAsync(CreateSystemPaymentMethodDto request);

        /// <summary>
        /// Update an existing system payment method (Admin only)
        /// </summary>
        Task<PaymentMethodResponse<SystemPaymentMethodDto>> UpdateSystemPaymentMethodAsync(int id, CreateSystemPaymentMethodDto request);

        /// <summary>
        /// Delete a system payment method (Admin only)
        /// </summary>
        Task<PaymentMethodResponse<bool>> DeleteSystemPaymentMethodAsync(int id);

        #endregion

        #region Merchant Payment Methods

        /// <summary>
        /// Get all payment methods for a specific merchant
        /// </summary>
        Task<PaymentMethodResponse<IEnumerable<MerchantPaymentMethodDto>>> GetMerchantPaymentMethodsAsync(Guid merchantId);

        /// <summary>
        /// Add a payment method to a merchant
        /// </summary>
        Task<PaymentMethodResponse<MerchantPaymentMethodDto>> AddMerchantPaymentMethodAsync(CreateMerchantPaymentMethodDto request);

        /// <summary>
        /// Update a merchant's payment method configuration
        /// </summary>
        Task<PaymentMethodResponse<MerchantPaymentMethodDto>> UpdateMerchantPaymentMethodAsync(int id, CreateMerchantPaymentMethodDto request);

        /// <summary>
        /// Remove a payment method from a merchant
        /// </summary>
        Task<PaymentMethodResponse<bool>> RemoveMerchantPaymentMethodAsync(int id);

        /// <summary>
        /// Enable/disable a payment method for a merchant
        /// </summary>
        Task<PaymentMethodResponse<bool>> ToggleMerchantPaymentMethodAsync(int id, bool isEnabled);

        #endregion
    }
}