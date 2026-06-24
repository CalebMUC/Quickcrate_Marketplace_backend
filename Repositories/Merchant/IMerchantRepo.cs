using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.Merchants;

namespace Minimart_Api.Repositories.Merchant
{
    public interface IMerchantRepo
    {
        #region Legacy Methods (Maintained for backward compatibility)
        Task<MerchantResponseStatus> AddMerchantsAsync(MerchantDto merchantDto);
        Task<MerchantResponseStatus> EditMerchantAsync(EditMerchantDto merchantDto);
        Task<ApiResponse<ApproveMerchantDto>> ApproveMerchantAsync(Guid MerchantId);
        Task<List<GetMerchantsDto>> GetMerchantsAsync();
        #endregion

        #region Enhanced Methods (Frontend compatible)
        /// <summary>
        /// Get merchants with filters, pagination, and sorting
        /// </summary>
        Task<MerchantsListResponse> GetMerchantsAsync(MerchantFilters filters);

        /// <summary>
        /// Get merchant by ID with full details
        /// </summary>
        Task<MerchantDetailDto?> GetMerchantByIdAsync(Guid id);

        /// <summary>
        /// Get pending merchants for approval queue
        /// </summary>
        Task<List<MerchantDetailDto>> GetPendingMerchantsAsync();

        /// <summary>
        /// Get merchant statistics
        /// </summary>
        Task<MerchantStatsDto> GetMerchantStatsAsync();

        /// <summary>
        /// Register a new merchant with enhanced data and file uploads
        /// </summary>
        Task<MerchantDetailDto> RegisterMerchantAsync(MerchantRegistrationDto dto);

        /// <summary>
        /// Update merchant information
        /// </summary>
        Task<MerchantDetailDto?> UpdateMerchantAsync(UpdateMerchantDto dto);

        /// <summary>
        /// Approve or reject a merchant with enhanced approval data
        /// </summary>
        Task<MerchantDetailDto?> ApproveMerchantAsync(MerchantApprovalDto approvalData);

        /// <summary>
        /// Suspend a merchant with reason
        /// </summary>
        Task<MerchantDetailDto?> SuspendMerchantAsync(SuspendMerchantDto suspensionData);

        /// <summary>
        /// Reactivate a suspended merchant
        /// </summary>
        Task<MerchantDetailDto?> ReactivateMerchantAsync(Guid id);

        /// <summary>
        /// Delete a merchant (soft delete)
        /// </summary>
        Task<bool> DeleteMerchantAsync(Guid id);
        #endregion
    }
}
