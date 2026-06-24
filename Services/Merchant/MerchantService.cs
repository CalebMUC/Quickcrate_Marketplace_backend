using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.Merchants;
using Minimart_Api.Repositories.Merchant;

namespace Minimart_Api.Services.Merchant
{
    public class MerchantService : IMerchantService
    {
        private readonly IMerchantRepo _merchantRepo;
        private readonly ILogger<MerchantService> _logger;

        public MerchantService(IMerchantRepo merchantRepo, ILogger<MerchantService> logger)
        {
            _merchantRepo = merchantRepo;
            _logger = logger;
        }

        #region Legacy Methods (Maintained for backward compatibility)

        public async Task<MerchantResponseStatus> AddMerchantsAsync(MerchantDto merchantDto)
        {
            return await _merchantRepo.AddMerchantsAsync(merchantDto);
        }

        public Task<MerchantResponseStatus> EditMerchantAsync(EditMerchantDto merchantDto)
        {
            return _merchantRepo.EditMerchantAsync(merchantDto);
        }

        public Task<ApiResponse<ApproveMerchantDto>> ApproveMerchantAsync(Guid MerchantId)
        {
            return _merchantRepo.ApproveMerchantAsync(MerchantId);
        }

        public Task<List<GetMerchantsDto>> GetMerchantsAsync()
        {
            return _merchantRepo.GetMerchantsAsync();
        }

        #endregion

        #region Enhanced Methods (Frontend compatible)

        public async Task<MerchantsListResponse> GetMerchantsAsync(MerchantFilters filters)
        {
            try
            {
                _logger.LogInformation("Getting merchants with filters: Search={Search}, Status={Status}, Page={Page}", 
                    filters.Search, filters.Status, filters.Page);

                return await _merchantRepo.GetMerchantsAsync(filters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MerchantService.GetMerchantsAsync with filters");
                return new MerchantsListResponse();
            }
        }

        public async Task<MerchantDetailDto?> GetMerchantByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Getting merchant by ID: {MerchantId}", id);
                return await _merchantRepo.GetMerchantByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MerchantService.GetMerchantByIdAsync for ID: {MerchantId}", id);
                return null;
            }
        }

        public async Task<List<MerchantDetailDto>> GetPendingMerchantsAsync()
        {
            try
            {
                _logger.LogInformation("Getting pending merchants");
                return await _merchantRepo.GetPendingMerchantsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MerchantService.GetPendingMerchantsAsync");
                return new List<MerchantDetailDto>();
            }
        }

        public async Task<MerchantStatsDto> GetMerchantStatsAsync()
        {
            try
            {
                _logger.LogInformation("Getting merchant statistics");
                return await _merchantRepo.GetMerchantStatsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MerchantService.GetMerchantStatsAsync");
                return new MerchantStatsDto();
            }
        }

        public async Task<MerchantDetailDto> RegisterMerchantAsync(MerchantRegistrationDto dto)
        {
            try
            {
                _logger.LogInformation("Registering new merchant: {BusinessName}", dto.BusinessName);

                // Add business logic validation if needed
                ValidateRegistrationDto(dto);

                return await _merchantRepo.RegisterMerchantAsync(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MerchantService.RegisterMerchantAsync for business: {BusinessName}", dto.BusinessName);
                throw;
            }
        }

        public async Task<MerchantDetailDto?> UpdateMerchantAsync(UpdateMerchantDto dto)
        {
            try
            {
                _logger.LogInformation("Updating merchant: {MerchantId}", dto.Id);

                // Add business logic validation if needed
                ValidateUpdateDto(dto);

                return await _merchantRepo.UpdateMerchantAsync(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MerchantService.UpdateMerchantAsync for ID: {MerchantId}", dto.Id);
                return null;
            }
        }

        public async Task<MerchantDetailDto?> ApproveMerchantAsync(MerchantApprovalDto approvalData)
        {
            try
            {
                _logger.LogInformation("Processing merchant approval: {MerchantId}, Status: {Status}", 
                    approvalData.MerchantId, approvalData.Status);

                // Add business logic for approval process
                ValidateApprovalDto(approvalData);

                return await _merchantRepo.ApproveMerchantAsync(approvalData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MerchantService.ApproveMerchantAsync for ID: {MerchantId}", approvalData.MerchantId);
                return null;
            }
        }

        public async Task<MerchantDetailDto?> SuspendMerchantAsync(SuspendMerchantDto suspensionData)
        {
            try
            {
                _logger.LogInformation("Suspending merchant: {MerchantId}, Reason: {Reason}", 
                    suspensionData.MerchantId, suspensionData.Reason);

                // Add business logic for suspension
                ValidateSuspensionDto(suspensionData);

                return await _merchantRepo.SuspendMerchantAsync(suspensionData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MerchantService.SuspendMerchantAsync for ID: {MerchantId}", suspensionData.MerchantId);
                return null;
            }
        }

        public async Task<MerchantDetailDto?> ReactivateMerchantAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Reactivating merchant: {MerchantId}", id);

                return await _merchantRepo.ReactivateMerchantAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MerchantService.ReactivateMerchantAsync for ID: {MerchantId}", id);
                return null;
            }
        }

        public async Task<bool> DeleteMerchantAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting merchant: {MerchantId}", id);

                // Add business logic for deletion (check for active orders, etc.)
                await ValidateMerchantDeletion(id);

                return await _merchantRepo.DeleteMerchantAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MerchantService.DeleteMerchantAsync for ID: {MerchantId}", id);
                return false;
            }
        }

        #endregion

        #region Private Validation Methods

        private void ValidateRegistrationDto(MerchantRegistrationDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.BusinessName))
                throw new ArgumentException("Business name is required");

            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new ArgumentException("Email is required");

            if (string.IsNullOrWhiteSpace(dto.BusinessRegistration))
                throw new ArgumentException("Business registration number is required");
        }

        private void ValidateUpdateDto(UpdateMerchantDto dto)
        {
            if (dto.Id == Guid.Empty)
                throw new ArgumentException("Valid merchant ID is required");
        }

        private void ValidateApprovalDto(MerchantApprovalDto dto)
        {
            if (dto.MerchantId == Guid.Empty)
                throw new ArgumentException("Valid merchant ID is required");

            if (string.IsNullOrWhiteSpace(dto.Status))
                throw new ArgumentException("Approval status is required");

            if (dto.Status.ToLower() != "approved" && dto.Status.ToLower() != "rejected")
                throw new ArgumentException("Status must be 'approved' or 'rejected'");

            if (dto.Status.ToLower() == "rejected" && string.IsNullOrWhiteSpace(dto.Reason))
                throw new ArgumentException("Rejection reason is required when rejecting a merchant");
        }

        private void ValidateSuspensionDto(SuspendMerchantDto dto)
        {
            if (dto.MerchantId == Guid.Empty)
                throw new ArgumentException("Valid merchant ID is required");

            if (string.IsNullOrWhiteSpace(dto.Reason))
                throw new ArgumentException("Suspension reason is required");
        }

        private async Task ValidateMerchantDeletion(Guid id)
        {
            // Add business logic to check if merchant can be deleted
            // For example, check for active orders, pending transactions, etc.
            // This is where you would implement business rules
        }

        #endregion
    }
}
