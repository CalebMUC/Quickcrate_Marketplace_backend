using Minimart_Api.DTOS.Payouts;
using Minimart_Api.Models;

namespace Minimart_Api.Services.Payouts
{
    /// <summary>
    /// Core business logic interface for payouts
    /// </summary>
    public interface IPayoutService
    {
        #region Merchant Payout Methods

        /// <summary>
        /// Get payout statistics for a specific merchant
        /// </summary>
        Task<PayoutStatsDto> GetMerchantPayoutStatsAsync(Guid merchantId);

        /// <summary>
        /// Get paginated list of payouts for a merchant
        /// </summary>
        Task<PagedResult<PayoutDto>> GetMerchantPayoutsAsync(
            Guid merchantId,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int page = 1,
            int pageSize = 20);

        /// <summary>
        /// Get detailed payout information by ID for a specific merchant
        /// </summary>
        Task<PayoutDetailDto?> GetPayoutByIdAsync(Guid payoutId, Guid merchantId);

        /// <summary>
        /// Get paginated list of payout transactions for a merchant
        /// </summary>
        Task<PagedResult<PayoutTransactionDto>> GetMerchantTransactionsAsync(
            Guid merchantId,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? payoutStatus = null,
            int page = 1,
            int pageSize = 20);

        #endregion

        #region Admin Payout Methods

        /// <summary>
        /// Get all payouts with filters (Admin only)
        /// </summary>
        Task<PagedResult<PayoutDto>> GetAllPayoutsAsync(PayoutFilters filters);

        /// <summary>
        /// Get detailed payout information by ID (Admin only)
        /// </summary>
        Task<PayoutDetailDto?> GetPayoutByIdAsync(Guid payoutId);

        /// <summary>
        /// Generate payouts for a specific period
        /// </summary>
        Task<GeneratePayoutsResponse> GenerateWeeklyPayoutsAsync(GeneratePayoutsRequest request);

        /// <summary>
        /// Update payout status (Admin only)
        /// </summary>
        Task<bool> UpdatePayoutStatusAsync(Guid payoutId, UpdatePayoutStatusRequest request, string updatedBy);

        /// <summary>
        /// Get merchant payout summaries (Admin dashboard)
        /// </summary>
        Task<PagedResult<MerchantPayoutSummaryDto>> GetMerchantPayoutSummariesAsync(
            int page = 1,
            int pageSize = 20,
            string? searchTerm = null);

        #endregion

        #region Utility Methods

        /// <summary>
        /// Calculate commission for an order amount
        /// </summary>
        decimal CalculateCommission(decimal orderAmount, decimal commissionRate);

        /// <summary>
        /// Validate payout status transition
        /// </summary>
        bool IsValidStatusTransition(string currentStatus, string newStatus);

        /// <summary>
        /// Get eligible orders for payout generation
        /// </summary>
        Task<List<Minimart_Api.Models.Order>> GetEligibleOrdersForPayoutAsync(
            DateTime periodStart, 
            DateTime periodEnd, 
            List<Guid>? merchantIds = null);

        /// <summary>
        /// Check if merchant has valid payment method for payouts
        /// </summary>
        Task<bool> MerchantHasValidPaymentMethodAsync(Guid merchantId);

        #endregion
    }
}