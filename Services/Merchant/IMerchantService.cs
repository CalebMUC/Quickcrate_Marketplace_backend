using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.Merchants;

namespace Minimart_Api.Services.Merchant
{
    public interface IMerchantService
    {
        #region Legacy Methods (Maintained for backward compatibility)
        public Task<MerchantResponseStatus> AddMerchantsAsync(MerchantDto merchantDto);
        public Task<MerchantResponseStatus> EditMerchantAsync(EditMerchantDto merchantDto);
        public Task<ApiResponse<ApproveMerchantDto>> ApproveMerchantAsync(Guid MerchantId);
        public Task<List<GetMerchantsDto>> GetMerchantsAsync();
        #endregion

        #region Enhanced Methods (Frontend compatible)
        /// <summary>
        /// Get merchants with filters, pagination, and sorting
        /// </summary>
        public Task<MerchantsListResponse> GetMerchantsAsync(MerchantFilters filters);

        /// <summary>
        /// Get merchant by ID with full details
        /// </summary>
        public Task<MerchantDetailDto?> GetMerchantByIdAsync(Guid id);

        /// <summary>
        /// Get pending merchants for approval queue
        /// </summary>
        public Task<List<MerchantDetailDto>> GetPendingMerchantsAsync();

        /// <summary>
        /// Get merchant statistics
        /// </summary>
        public Task<MerchantStatsDto> GetMerchantStatsAsync();

        /// <summary>
        /// Register a new merchant with enhanced data and file uploads
        /// </summary>
        public Task<MerchantDetailDto> RegisterMerchantAsync(MerchantRegistrationDto dto);

        /// <summary>
        /// Update merchant information
        /// </summary>
        public Task<MerchantDetailDto?> UpdateMerchantAsync(UpdateMerchantDto dto);

        /// <summary>
        /// Approve or reject a merchant with enhanced approval data
        /// </summary>
        public Task<MerchantDetailDto?> ApproveMerchantAsync(MerchantApprovalDto approvalData);

        /// <summary>
        /// Suspend a merchant with reason
        /// </summary>
        public Task<MerchantDetailDto?> SuspendMerchantAsync(SuspendMerchantDto suspensionData);

        /// <summary>
        /// Reactivate a suspended merchant
        /// </summary>
        public Task<MerchantDetailDto?> ReactivateMerchantAsync(Guid id);

        /// <summary>
        /// Delete a merchant (soft delete)
        /// </summary>
        public Task<bool> DeleteMerchantAsync(Guid id);
        #endregion
    }
}
