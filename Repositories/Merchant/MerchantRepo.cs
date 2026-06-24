using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Minimart_Api.Data;
using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.Merchants;
using Minimart_Api.Models;
using Minimart_Api.Services.EmailServices;
using Minimart_Api.Services.PasswordGenerator;

namespace Minimart_Api.Repositories.Merchant
{
    public class MerchantRepo : IMerchantRepo
    {
        private readonly MinimartDBContext _dbContext;
        private readonly ILogger<MerchantRepo> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IPasswordGeneratorService _passwordGeneratorService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public MerchantRepo(
            MinimartDBContext dBContext,
            ILogger<MerchantRepo> logger,
            UserManager<ApplicationUser> userManager,
            IPasswordGeneratorService passwordGeneratorService,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _dbContext = dBContext;
            _logger = logger;
            _userManager = userManager;
            _passwordGeneratorService = passwordGeneratorService;
            _emailService = emailService;
            _configuration = configuration;
        }

        #region Legacy Methods (Maintained for backward compatibility)

        public async Task<List<GetMerchantsDto>> GetMerchantsAsync()
        {
            try
            {
                var merchants = await _dbContext.Merchants
                    .Select(m => new GetMerchantsDto
                    {
                        MerchantID = m.MerchantID,
                        BusinessName = m.BusinessName,
                        BusinessType = m.BusinessType,
                        BusinessRegistrationNo = m.BusinessRegistrationNo,
                        KRAPIN = m.KRAPIN,
                        BusinessNature = m.BusinessNature,
                        BusinessCategory = m.BusinessCategory,
                        MerchantName = m.MerchantName,
                        Email = m.Email,
                        Phone = m.Phone,
                        Address = m.Address,
                        SocialMedia = m.SocialMedia,
                        TermsAndCondition = m.TermsAndCondition,
                        DeliveryMethod = m.DeliveryMethod,
                        ReturnPolicy = m.ReturnPolicy,
                        Status = m.Status
                    })
                    .ToListAsync();
                return merchants;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get merchants: {Message}", ex.Message);
                return new List<GetMerchantsDto>();
            }
        }

        public async Task<MerchantResponseStatus> EditMerchantAsync(EditMerchantDto merchantDto)
        {
            try
            {
                var existingMerchant = await _dbContext.Merchants.FirstOrDefaultAsync(m => m.MerchantID == merchantDto.MerchantID);
                if (existingMerchant == null)
                {
                    return new MerchantResponseStatus
                    {
                        ResponseCode = 404,
                        ResponseMessage = "Merchant not found."
                    };
                }
                UpdateMerchantFromDto(existingMerchant, merchantDto);
                _dbContext.Merchants.Update(existingMerchant);
                await _dbContext.SaveChangesAsync();
                return new MerchantResponseStatus
                {
                    ResponseCode = 200,
                    ResponseMessage = "Merchant updated successfully."
                };


            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to Edit Merchant: {Message}", ex.Message);
                return new MerchantResponseStatus
                {
                    ResponseCode = 500,
                    ResponseMessage = "An error occurred while updating the merchant."
                };
            }
        }

        public async Task<MerchantResponseStatus> AddMerchantsAsync(MerchantDto merchantDto)
        {
            try
            {
                //check if merchant with same BusinessRegistrationNo or KRAPIN exists
                var existingMerchant = _dbContext.Merchants
                    .FirstOrDefault(m => m.BusinessRegistrationNo == merchantDto.BusinessRegistrationNo || m.KRAPIN == merchantDto.KRAPIN);
                if (existingMerchant != null)
                {
                    return new MerchantResponseStatus
                    {
                        ResponseCode = 409,
                        ResponseMessage = "Merchant with the same Business Registration Number or KRAPIN already exists."
                    };
                }
                var newMerchant = new Merchants
                {
                    MerchantID = Guid.NewGuid(),
                    BusinessName = merchantDto.BusinessName,
                    BusinessType = merchantDto.BusinessType,
                    BusinessRegistrationNo = merchantDto.BusinessRegistrationNo,
                    KRAPIN = merchantDto.KRAPIN,
                    BusinessNature = merchantDto.BusinessNature,
                    BusinessCategory = merchantDto.BusinessCategory,
                    MerchantName = merchantDto.MerchantName,
                    Email = merchantDto.Email,
                    Phone = merchantDto.Phone,
                    Address = merchantDto.Address,
                    //SocialMedia = merchantDto.SocialMedia,
                    //BankName = merchantDto.BankName,
                    //BankAccountNo = merchantDto.BankAccountNo,
                    //BankAccountName = merchantDto.BankAccountName,
                    //MpesaPaybill = merchantDto.MpesaPaybill,
                    //MpesaTillNumber = merchantDto.MpesaTillNumber,
                    //PreferredPaymentChannel = merchantDto.PreferredPaymentChannel,
                    //KRAPINCertificate = merchantDto.KRAPINCertificate,
                    //BusinessRegistrationCertificate = merchantDto.BusinessRegistrationCertificate,
                    TermsAndCondition = merchantDto.TermsAndCondition,
                    DeliveryMethod = merchantDto.DeliveryMethod,
                    ReturnPolicy = merchantDto.ReturnPolicy,
                    Status = merchantDto.Status ?? "Active",
                    RegistrationDate = DateTime.UtcNow
                };

                await _dbContext.Merchants.AddAsync(newMerchant);
                await _dbContext.SaveChangesAsync();
                return new MerchantResponseStatus
                {
                    ResponseCode = 201,
                    ResponseMessage = "Merchant added successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to Add Merchant :", ex.Message);
                return new MerchantResponseStatus
                {
                    ResponseCode = 500,
                    ResponseMessage = "An error occurred while adding the merchant."
                };
            }
        }

        public async Task<ApiResponse<ApproveMerchantDto>> ApproveMerchantAsync(Guid MerchantId)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    // 1. Validate merchant exists and is pending
                    var merchant = await _dbContext.Merchants.FirstOrDefaultAsync(m => m.MerchantID == MerchantId);
                    if (merchant == null)
                        return ApiResponse<ApproveMerchantDto>.CreateError("Merchant not found.");

                    if (merchant.Status != "Pending Approval")
                        return ApiResponse<ApproveMerchantDto>.CreateError("Merchant is not in a pending state.");

                    // 2. Check if user account already exists
                    var existingUser = await _userManager.FindByEmailAsync(merchant.Email);
                    if (existingUser != null)
                    {
                        _logger.LogWarning($"User account already exists for email: {merchant.Email}");
                        return ApiResponse<ApproveMerchantDto>.CreateError("User account already exists for this email");
                    }

                    // 3. Generate temporary password
                    var temporaryPassword = _passwordGeneratorService.GenerateTemporaryPassword();

                    // 4. Create user account
                    var user = new ApplicationUser
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = merchant.Email,
                        Email = merchant.Email,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow,
                        IsTemporaryPassword = true,
                        LastPasswordReset = DateTime.UtcNow
                    };

                    _dbContext.Entry(user).State = EntityState.Detached;

                    var createResult = await _userManager.CreateAsync(user, temporaryPassword);
                    if (!createResult.Succeeded)
                    {
                        var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                        _logger.LogError($"Failed to create user account for merchant {MerchantId}: {errors}");
                        return ApiResponse<ApproveMerchantDto>.CreateError($"Failed to create user account: {errors}");
                    }

                    // 5. Assign Merchant role
                    var roleResult = await _userManager.AddToRoleAsync(user, "Merchant");
                    if (!roleResult.Succeeded)
                    {
                        var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                        _logger.LogError($"Failed to assign Merchant role to user {user.Id}: {errors}");
                    }

                    // 6. Update merchant
                    merchant.Status = "Approved";
                    merchant.ApplicationUserId = user.Id;
                    await _dbContext.SaveChangesAsync();

                    // 7. Send welcome email
                    var dashboardUrl = _configuration["Application:MerchantDashboardUrl"] ?? "https://dashboard.quickcrate.co.ke";
                    var emailSent = await _emailService.SendMerchantWelcomeEmailAsync(
                        merchant.Email,
                        merchant.BusinessName,
                        merchant.Email,
                        temporaryPassword,
                        dashboardUrl
                    );

                    await transaction.CommitAsync();

                    _logger.LogInformation($"Merchant {MerchantId} approved successfully");

                    var merchantDto = new ApproveMerchantDto
                    {
                        MerchantID = merchant.MerchantID,
                        BusinessName = merchant.BusinessName,
                        Email = merchant.Email,
                        Status = merchant.Status,
                        UserId = user.Id,
                        AccountCreated = true,
                        EmailSent = emailSent
                    };

                    return ApiResponse<ApproveMerchantDto>.CreateSuccess(merchantDto, "Merchant approved successfully");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, $"Error approving merchant {MerchantId}");
                    return ApiResponse<ApproveMerchantDto>.CreateError($"An error occurred while approving merchant: {ex.Message}");
                }
            });
        }

        #endregion

        #region Enhanced Methods (Frontend compatible)

        public async Task<MerchantsListResponse> GetMerchantsAsync(MerchantFilters filters)
        {
            return await GetMerchantsAsync(filters, includePaymentMethods: true);
        }

        public async Task<MerchantsListResponse> GetMerchantsAsync(MerchantFilters filters, bool includePaymentMethods = false)
        {
            try
            {
                var query = _dbContext.Merchants.AsQueryable();

                // Include payment methods if requested
                if (includePaymentMethods)
                {
                    query = query
                        .Include(m => m.MerchantPaymentMethods)
                        .ThenInclude(mpm => mpm.PaymentMethod);
                }

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(filters.Search))
                {
                    var searchTerm = filters.Search.ToLower();
                    query = query.Where(m =>
                        m.BusinessName!.ToLower().Contains(searchTerm) ||
                        m.Email!.ToLower().Contains(searchTerm) ||
                        m.MerchantName!.ToLower().Contains(searchTerm) ||
                        m.BusinessRegistrationNo!.ToLower().Contains(searchTerm));
                }

                // Apply status filter
                if (!string.IsNullOrWhiteSpace(filters.Status))
                {
                    query = query.Where(m => m.Status == filters.Status);
                }

                // Apply sorting
                query = filters.SortBy.ToLower() switch
                {
                    "businessname" => filters.SortOrder.ToLower() == "desc" 
                        ? query.OrderByDescending(m => m.BusinessName)
                        : query.OrderBy(m => m.BusinessName),
                    "createdat" => filters.SortOrder.ToLower() == "desc"
                        ? query.OrderByDescending(m => m.RegistrationDate)
                        : query.OrderBy(m => m.RegistrationDate),
                    "status" => filters.SortOrder.ToLower() == "desc"
                        ? query.OrderByDescending(m => m.Status)
                        : query.OrderBy(m => m.Status),
                    _ => query.OrderBy(m => m.BusinessName)
                };

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply pagination and materialize results
                var merchants = await query
                    .Skip((filters.Page - 1) * filters.PageSize)
                    .Take(filters.PageSize)
                    .ToListAsync();

                // Map to DTOs with payment methods included if requested
                var merchantDtos = merchants.Select(m => MapToMerchantDetailDto(m, includePaymentMethods)).ToList();

                return new MerchantsListResponse
                {
                    Merchants = merchantDtos,
                    TotalCount = totalCount,
                    Page = filters.Page,
                    PageSize = filters.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting merchants with filters");
                return new MerchantsListResponse();
            }
        }

        public async Task<MerchantDetailDto?> GetMerchantByIdAsync(Guid id)
        {
            try
            {
                var merchant = await _dbContext.Merchants
                    .Include(m => m.MerchantPaymentMethods)
                    .ThenInclude(mpm => mpm.PaymentMethod)
                    .Where(m => m.MerchantID == id)
                    .FirstOrDefaultAsync();

                if (merchant == null) return null;

                return MapToMerchantDetailDto(merchant, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting merchant by ID: {Id}", id);
                return null;
            }
        }

        public async Task<List<MerchantDetailDto>> GetPendingMerchantsAsync()
        {
            try
            {
                var pendingMerchants = await _dbContext.Merchants
                    .Include(m => m.MerchantPaymentMethods)
                    .ThenInclude(mpm => mpm.PaymentMethod)
                    .Where(m => m.Status == "Pending" || m.Status == "Pending Approval")
                    .OrderBy(m => m.RegistrationDate)
                    .ToListAsync();

                // Map to DTOs with payment methods included
                return pendingMerchants.Select(m => MapToMerchantDetailDto(m, true)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending merchants");
                return new List<MerchantDetailDto>();
            }
        }

        public async Task<MerchantStatsDto> GetMerchantStatsAsync()
        {
            try
            {
                var stats = await _dbContext.Merchants
                    .GroupBy(m => 1)
                    .Select(g => new MerchantStatsDto
                    {
                        Total = g.Count(),
                        Pending = g.Count(m => m.Status == "Pending" || m.Status == "Pending Approval"),
                        Approved = g.Count(m => m.Status == "Approved"),
                        Active = g.Count(m => m.Status == "Active"),
                        Suspended = g.Count(m => m.Status == "Suspended"),
                        Rejected = g.Count(m => m.Status == "Rejected")
                    })
                    .FirstOrDefaultAsync();

                return stats ?? new MerchantStatsDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting merchant statistics");
                return new MerchantStatsDto();
            }
        }

        public async Task<MerchantDetailDto> RegisterMerchantAsync(MerchantRegistrationDto dto)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    // Check for existing merchant
                    var existingMerchant = await _dbContext.Merchants
                        .FirstOrDefaultAsync(m => m.BusinessRegistrationNo == dto.BusinessRegistration || m.Email == dto.Email);
                    
                    if (existingMerchant != null)
                    {
                        throw new InvalidOperationException("Merchant with the same business registration or email already exists.");
                    }

                    var newMerchant = new Merchants
                    {
                        MerchantID = Guid.NewGuid(),
                        BusinessName = dto.BusinessName,
                        BusinessType = dto.BusinessType,
                        BusinessRegistrationNo = dto.BusinessRegistration,
                        KRAPIN = dto.TaxId,
                        BusinessNature = dto.BusinessNature,
                        BusinessCategory = dto.BusinessCategory,
                        MerchantName = dto.ContactPerson,
                        Email = dto.Email,
                        Phone = dto.Phone,
                        Address = $"{dto.Address}, {dto.City}, {dto.Country}",
                        SocialMedia = dto.SocialMedia,
                        DeliveryMethod = dto.DeliveryMethod,
                        ReturnPolicy = dto.ReturnPolicy,
                        TermsAndCondition = dto.TermsAndCondition,
                        Status = "Pending Approval",
                        RegistrationDate = DateTime.UtcNow,
                        Documents = dto.Documents ?? new List<string>()
                    };

                    await _dbContext.Merchants.AddAsync(newMerchant);
                    await _dbContext.SaveChangesAsync();

                    // Handle payment methods configuration
                    if (dto.PaymentMethods != null && dto.PaymentMethods.Any())
                    {
                        foreach (var paymentMethodDto in dto.PaymentMethods)
                        {
                            // Verify that the payment method exists
                            var paymentMethodExists = await _dbContext.PaymentMethods
                                .AnyAsync(pm => pm.PaymentMethodID == paymentMethodDto.PaymentMethodId);

                            if (paymentMethodExists)
                            {
                                var merchantPaymentMethod = new MerchantPaymentMethod
                                {
                                    MerchantId = newMerchant.MerchantID,
                                    PaymentMethodId = paymentMethodDto.PaymentMethodId,
                                    Configuration = paymentMethodDto.Configuration,
                                    IsEnabled = paymentMethodDto.IsEnabled,
                                    CreatedAt = DateTime.UtcNow
                                };

                                await _dbContext.MerchantPaymentMethods.AddAsync(merchantPaymentMethod);
                            }
                            else
                            {
                                _logger.LogWarning("Payment method with ID {PaymentMethodId} does not exist for merchant {MerchantId}", 
                                    paymentMethodDto.PaymentMethodId, newMerchant.MerchantID);
                            }
                        }

                        await _dbContext.SaveChangesAsync();
                    }

                    // Handle document metadata (if provided)
                    if (dto.Documents != null && dto.Documents.Any())
                    {
                        _logger.LogInformation("Document URLs received for merchant {MerchantId}: {DocumentCount} documents", 
                            newMerchant.MerchantID, dto.Documents.Count);
                    }

                    await transaction.CommitAsync();

                    return await GetMerchantDetailWithPaymentMethods(newMerchant.MerchantID);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error registering merchant");
                    throw;
                }
            });
        }

        public async Task<MerchantDetailDto?> UpdateMerchantAsync(UpdateMerchantDto dto)
        {
            try
            {
                var merchant = await _dbContext.Merchants
                    .Include(m => m.MerchantPaymentMethods)
                    .ThenInclude(mpm => mpm.PaymentMethod)
                    .FirstOrDefaultAsync(m => m.MerchantID == dto.Id);
                    
                if (merchant == null) return null;

                // Update only provided fields
                if (!string.IsNullOrWhiteSpace(dto.BusinessName))
                    merchant.BusinessName = dto.BusinessName;
                if (!string.IsNullOrWhiteSpace(dto.BusinessRegistration))
                    merchant.BusinessRegistrationNo = dto.BusinessRegistration;
                if (!string.IsNullOrWhiteSpace(dto.TaxId))
                    merchant.KRAPIN = dto.TaxId;
                if (!string.IsNullOrWhiteSpace(dto.ContactPerson))
                    merchant.MerchantName = dto.ContactPerson;
                if (!string.IsNullOrWhiteSpace(dto.Email))
                    merchant.Email = dto.Email;
                if (!string.IsNullOrWhiteSpace(dto.Phone))
                    merchant.Phone = dto.Phone;
                if (!string.IsNullOrWhiteSpace(dto.Address))
                    merchant.Address = dto.Address;

                // Update Documents if provided
                if (dto.Documents != null)
                {
                    merchant.Documents = dto.Documents.ToList();
                }

                await _dbContext.SaveChangesAsync();

                return MapToMerchantDetailDto(merchant, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating merchant: {Id}", dto.Id);
                return null;
            }
        }

        public async Task<MerchantDetailDto?> ApproveMerchantAsync(MerchantApprovalDto approvalData)
        {
            try
            {
                var merchant = await _dbContext.Merchants
                    .Include(m => m.MerchantPaymentMethods)
                    .ThenInclude(mpm => mpm.PaymentMethod)
                    .FirstOrDefaultAsync(m => m.MerchantID == approvalData.MerchantId);
                    
                if (merchant == null) return null;

                merchant.Status = approvalData.Status == "approved" ? "Approved" : "Rejected";
                
                if (approvalData.Status == "rejected" && !string.IsNullOrWhiteSpace(approvalData.Reason))
                {
                    // Store rejection reason (you might want to add this field to the model)
                }

                await _dbContext.SaveChangesAsync();

                return MapToMerchantDetailDto(merchant, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving/rejecting merchant: {Id}", approvalData.MerchantId);
                return null;
            }
        }

        public async Task<MerchantDetailDto?> SuspendMerchantAsync(SuspendMerchantDto suspensionData)
        {
            try
            {
                var merchant = await _dbContext.Merchants
                    .Include(m => m.MerchantPaymentMethods)
                    .ThenInclude(mpm => mpm.PaymentMethod)
                    .FirstOrDefaultAsync(m => m.MerchantID == suspensionData.MerchantId);
                    
                if (merchant == null) return null;

                merchant.Status = "Suspended";
                // TODO: Store suspension reason in a separate table or field

                await _dbContext.SaveChangesAsync();

                return MapToMerchantDetailDto(merchant, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suspending merchant: {Id}", suspensionData.MerchantId);
                return null;
            }
        }

        public async Task<MerchantDetailDto?> ReactivateMerchantAsync(Guid id)
        {
            try
            {
                var merchant = await _dbContext.Merchants
                    .Include(m => m.MerchantPaymentMethods)
                    .ThenInclude(mpm => mpm.PaymentMethod)
                    .FirstOrDefaultAsync(m => m.MerchantID == id);
                    
                if (merchant == null) return null;

                merchant.Status = "Active";

                await _dbContext.SaveChangesAsync();

                return MapToMerchantDetailDto(merchant, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating merchant: {Id}", id);
                return null;
            }
        }

        public async Task<bool> DeleteMerchantAsync(Guid id)
        {
            try
            {
                var merchant = await _dbContext.Merchants.FirstOrDefaultAsync(m => m.MerchantID == id);
                if (merchant == null) return false;

                // Soft delete by setting status
                merchant.Status = "Deleted";

                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting merchant: {Id}", id);
                return false;
            }
        }

        #endregion

        #region Private Helper Methods

        private void UpdateMerchantFromDto(Merchants merchant, EditMerchantDto merchantDto)
        {
            merchant.BusinessName = merchantDto.BusinessName;
            merchant.BusinessType = merchantDto.BusinessType;
            merchant.BusinessRegistrationNo = merchantDto.BusinessRegistrationNo;
            merchant.KRAPIN = merchantDto.KRAPIN;
            merchant.BusinessNature = merchantDto.BusinessNature;
            merchant.BusinessCategory = merchantDto.BusinessCategory;
            merchant.MerchantName = merchantDto.MerchantName;
            merchant.Email = merchantDto.Email;
            merchant.Phone = merchantDto.Phone;
            merchant.Address = merchantDto.Address;
            merchant.SocialMedia = merchantDto.SocialMedia;
            merchant.TermsAndCondition = merchantDto.TermsAndCondition;
            merchant.DeliveryMethod = merchantDto.DeliveryMethod;
            merchant.ReturnPolicy = merchantDto.ReturnPolicy;
            if (!string.IsNullOrEmpty(merchantDto.Status))
            {
                merchant.Status = merchantDto.Status;
            }
        }

        private async Task<MerchantDetailDto> GetMerchantDetailWithPaymentMethods(Guid merchantId)
        {
            var merchant = await _dbContext.Merchants
                .Include(m => m.MerchantPaymentMethods)
                .ThenInclude(mpm => mpm.PaymentMethod)
                .FirstOrDefaultAsync(m => m.MerchantID == merchantId);

            if (merchant == null)
                throw new InvalidOperationException($"Merchant with ID {merchantId} not found");

            return MapToMerchantDetailDto(merchant, true);
        }

        private MerchantDetailDto MapToMerchantDetailDto(Merchants merchant, bool includePaymentMethods = false)
        {
            var result = new MerchantDetailDto
            {
                Id = merchant.MerchantID,
                UserId = merchant.ApplicationUserId,
                BusinessName = merchant.BusinessName ?? "",
                BusinessRegistration = merchant.BusinessRegistrationNo ?? "",
                TaxId = merchant.KRAPIN,
                ContactPerson = merchant.MerchantName ?? "",
                Email = merchant.Email ?? "",
                Phone = merchant.Phone ?? "",
                Address = merchant.Address ?? "",
                City = ExtractCityFromAddress(merchant.Address),
                Country = ExtractCountryFromAddress(merchant.Address),
                Status = merchant.Status ?? "Pending",
                CreatedAt = merchant.RegistrationDate ?? DateTime.UtcNow,
                BusinessNature = merchant.BusinessNature,
                BusinessCategory = merchant.BusinessCategory,
                BusinessType = merchant.BusinessType,
                SocialMedia = merchant.SocialMedia,
                DeliveryMethod = merchant.DeliveryMethod,
                ReturnPolicy = merchant.ReturnPolicy,
                TermsAndCondition = merchant.TermsAndCondition,
                Documents = merchant.Documents?.ToList() ?? new List<string>()
            };

            // Include payment methods if requested and available
            if (includePaymentMethods && merchant.MerchantPaymentMethods != null)
            {
                result.PaymentMethods = merchant.MerchantPaymentMethods
                    .Where(mpm => mpm.PaymentMethod != null)
                    .Select(mpm => new MerchantPaymentMethodDetailDto
                    {
                        Id = mpm.Id,
                        PaymentMethodId = mpm.PaymentMethodId,
                        PaymentMethodName = mpm.PaymentMethod.Name,
                        Configuration = mpm.Configuration ?? "",
                        IsEnabled = mpm.IsEnabled,
                        CreatedAt = mpm.CreatedAt,
                        UpdatedAt = mpm.UpdatedAt
                    })
                    .ToList();
            }

            return result;
        }

        private string ExtractCityFromAddress(string? address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return "N/A";

            // Simple extraction - assumes format: "address, city, country"
            var parts = address.Split(',');
            return parts.Length >= 2 ? parts[^2].Trim() : "N/A";
        }

        private string ExtractCountryFromAddress(string? address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return "Kenya"; // Default

            // Simple extraction - assumes format: "address, city, country"
            var parts = address.Split(',');
            return parts.Length >= 3 ? parts[^1].Trim() : "Kenya";
        }

        #endregion
    }
}
