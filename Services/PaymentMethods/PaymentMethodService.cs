using Microsoft.EntityFrameworkCore;
using Minimart_Api.Data;
using Minimart_Api.DTOS.PaymentMethods;
using Minimart_Api.Models;

namespace Minimart_Api.Services.PaymentMethods
{
    /// <summary>
    /// Service implementation for managing payment methods
    /// </summary>
    public class PaymentMethodService : IPaymentMethodService
    {
        private readonly MinimartDBContext _context;
        private readonly ILogger<PaymentMethodService> _logger;

        public PaymentMethodService(MinimartDBContext context, ILogger<PaymentMethodService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region System Payment Methods

        public async Task<PaymentMethodResponse<IEnumerable<SystemPaymentMethodDto>>> GetSystemPaymentMethodsAsync()
        {
            try
            {
                var paymentMethods = await _context.PaymentMethods
                    .Where(pm => pm.IsActive)
                    .OrderBy(pm => pm.Name)
                    .Select(pm => new SystemPaymentMethodDto
                    {
                        PaymentMethodId = pm.PaymentMethodID,
                        Name = pm.Name,
                        Description = pm.Description,
                        IsActive = pm.IsActive,
                        ImageUrl = pm.ImageUrl,
                        CreatedDate = pm.CreatedDate
                    })
                    .ToListAsync();

                return PaymentMethodResponse<IEnumerable<SystemPaymentMethodDto>>.CreateSuccess(
                    paymentMethods, 
                    $"Retrieved {paymentMethods.Count} system payment methods"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system payment methods");
                return PaymentMethodResponse<IEnumerable<SystemPaymentMethodDto>>.CreateError(
                    "Failed to retrieve system payment methods"
                );
            }
        }

        public async Task<PaymentMethodResponse<SystemPaymentMethodDto>> GetSystemPaymentMethodByIdAsync(int id)
        {
            try
            {
                var paymentMethod = await _context.PaymentMethods
                    .Where(pm => pm.PaymentMethodID == id)
                    .Select(pm => new SystemPaymentMethodDto
                    {
                        PaymentMethodId = pm.PaymentMethodID,
                        Name = pm.Name,
                        Description = pm.Description,
                        IsActive = pm.IsActive,
                        ImageUrl = pm.ImageUrl,
                        CreatedDate = pm.CreatedDate
                    })
                    .FirstOrDefaultAsync();

                if (paymentMethod == null)
                {
                    return PaymentMethodResponse<SystemPaymentMethodDto>.CreateError("Payment method not found");
                }

                return PaymentMethodResponse<SystemPaymentMethodDto>.CreateSuccess(
                    paymentMethod,
                    "Payment method retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment method {Id}", id);
                return PaymentMethodResponse<SystemPaymentMethodDto>.CreateError(
                    "Failed to retrieve payment method"
                );
            }
        }

        public async Task<PaymentMethodResponse<SystemPaymentMethodDto>> CreateSystemPaymentMethodAsync(CreateSystemPaymentMethodDto request)
        {
            try
            {
                // Check for duplicate names
                var existingMethod = await _context.PaymentMethods
                    .AnyAsync(pm => pm.Name.ToLower() == request.Name.ToLower());

                if (existingMethod)
                {
                    return PaymentMethodResponse<SystemPaymentMethodDto>.CreateError(
                        "A payment method with this name already exists"
                    );
                }

                var paymentMethod = new Models.PaymentMethods
                {
                    Name = request.Name.Trim(),
                    Description = request.Description?.Trim() ?? string.Empty,
                    ImageUrl = request.ImageUrl?.Trim() ?? string.Empty,
                    IsActive = request.IsActive,
                    CreatedDate = DateTime.UtcNow
                };

                _context.PaymentMethods.Add(paymentMethod);
                await _context.SaveChangesAsync();

                var result = new SystemPaymentMethodDto
                {
                    PaymentMethodId = paymentMethod.PaymentMethodID,
                    Name = paymentMethod.Name,
                    Description = paymentMethod.Description,
                    IsActive = paymentMethod.IsActive,
                    ImageUrl = paymentMethod.ImageUrl,
                    CreatedDate = paymentMethod.CreatedDate
                };

                _logger.LogInformation("Created system payment method {Name} with ID {Id}", 
                    paymentMethod.Name, paymentMethod.PaymentMethodID);

                return PaymentMethodResponse<SystemPaymentMethodDto>.CreateSuccess(
                    result,
                    "Payment method created successfully"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating system payment method");
                return PaymentMethodResponse<SystemPaymentMethodDto>.CreateError(
                    "Failed to create payment method"
                );
            }
        }

        public async Task<PaymentMethodResponse<SystemPaymentMethodDto>> UpdateSystemPaymentMethodAsync(int id, CreateSystemPaymentMethodDto request)
        {
            try
            {
                var paymentMethod = await _context.PaymentMethods.FindAsync(id);
                if (paymentMethod == null)
                {
                    return PaymentMethodResponse<SystemPaymentMethodDto>.CreateError("Payment method not found");
                }

                // Check for duplicate names (excluding current record)
                var existingMethod = await _context.PaymentMethods
                    .AnyAsync(pm => pm.PaymentMethodID != id && pm.Name.ToLower() == request.Name.ToLower());

                if (existingMethod)
                {
                    return PaymentMethodResponse<SystemPaymentMethodDto>.CreateError(
                        "A payment method with this name already exists"
                    );
                }

                paymentMethod.Name = request.Name.Trim();
                paymentMethod.Description = request.Description?.Trim() ?? string.Empty;
                paymentMethod.ImageUrl = request.ImageUrl?.Trim() ?? string.Empty;
                paymentMethod.IsActive = request.IsActive;

                await _context.SaveChangesAsync();

                var result = new SystemPaymentMethodDto
                {
                    PaymentMethodId = paymentMethod.PaymentMethodID,
                    Name = paymentMethod.Name,
                    Description = paymentMethod.Description,
                    IsActive = paymentMethod.IsActive,
                    ImageUrl = paymentMethod.ImageUrl,
                    CreatedDate = paymentMethod.CreatedDate
                };

                _logger.LogInformation("Updated system payment method {Id}", id);

                return PaymentMethodResponse<SystemPaymentMethodDto>.CreateSuccess(
                    result,
                    "Payment method updated successfully"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating system payment method {Id}", id);
                return PaymentMethodResponse<SystemPaymentMethodDto>.CreateError(
                    "Failed to update payment method"
                );
            }
        }

        public async Task<PaymentMethodResponse<bool>> DeleteSystemPaymentMethodAsync(int id)
        {
            try
            {
                var paymentMethod = await _context.PaymentMethods.FindAsync(id);
                if (paymentMethod == null)
                {
                    return PaymentMethodResponse<bool>.CreateError("Payment method not found");
                }

                // Check if payment method is used by any merchants
                var isUsedByMerchants = await _context.Set<MerchantPaymentMethod>()
                    .AnyAsync(mpm => mpm.PaymentMethodId == id);

                if (isUsedByMerchants)
                {
                    return PaymentMethodResponse<bool>.CreateError(
                        "Cannot delete payment method as it is currently used by merchants"
                    );
                }

                _context.PaymentMethods.Remove(paymentMethod);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted system payment method {Id}", id);

                return PaymentMethodResponse<bool>.CreateSuccess(
                    true,
                    "Payment method deleted successfully"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting system payment method {Id}", id);
                return PaymentMethodResponse<bool>.CreateError(
                    "Failed to delete payment method"
                );
            }
        }

        #endregion

        #region Merchant Payment Methods

        public async Task<PaymentMethodResponse<IEnumerable<MerchantPaymentMethodDto>>> GetMerchantPaymentMethodsAsync(Guid merchantId)
        {
            try
            {
                var merchantPaymentMethods = await _context.Set<MerchantPaymentMethod>()
                    .Include(mpm => mpm.PaymentMethod)
                    .Where(mpm => mpm.MerchantId == merchantId)
                    .OrderBy(mpm => mpm.PaymentMethod.Name)
                    .Select(mpm => new MerchantPaymentMethodDto
                    {
                        Id = mpm.Id,
                        MerchantId = mpm.MerchantId,
                        PaymentMethodId = mpm.PaymentMethodId,
                        PaymentMethodName = mpm.PaymentMethod.Name,
                        PaymentMethodDescription = mpm.PaymentMethod.Description,
                        ImageUrl = mpm.PaymentMethod.ImageUrl,
                        Configuration = mpm.Configuration ?? string.Empty,
                        IsEnabled = mpm.IsEnabled,
                        CreatedAt = mpm.CreatedAt,
                        UpdatedAt = mpm.UpdatedAt
                    })
                    .ToListAsync();

                return PaymentMethodResponse<IEnumerable<MerchantPaymentMethodDto>>.CreateSuccess(
                    merchantPaymentMethods,
                    $"Retrieved {merchantPaymentMethods.Count} payment methods for merchant {merchantId}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment methods for merchant {MerchantId}", merchantId);
                return PaymentMethodResponse<IEnumerable<MerchantPaymentMethodDto>>.CreateError(
                    "Failed to retrieve merchant payment methods"
                );
            }
        }

        public async Task<PaymentMethodResponse<MerchantPaymentMethodDto>> AddMerchantPaymentMethodAsync(CreateMerchantPaymentMethodDto request)
        {
            try
            {
                // Validate merchant exists
                var merchantExists = await _context.Merchants
                    .AnyAsync(m => m.MerchantID == request.MerchantId);

                if (!merchantExists)
                {
                    return PaymentMethodResponse<MerchantPaymentMethodDto>.CreateError("Merchant not found");
                }

                // Validate payment method exists
                var paymentMethodExists = await _context.PaymentMethods
                    .AnyAsync(pm => pm.PaymentMethodID == request.PaymentMethodId);

                if (!paymentMethodExists)
                {
                    return PaymentMethodResponse<MerchantPaymentMethodDto>.CreateError("Payment method not found");
                }

                // Check if merchant already has this payment method
                var existingMerchantPaymentMethod = await _context.Set<MerchantPaymentMethod>()
                    .AnyAsync(mpm => mpm.MerchantId == request.MerchantId && mpm.PaymentMethodId == request.PaymentMethodId);

                if (existingMerchantPaymentMethod)
                {
                    return PaymentMethodResponse<MerchantPaymentMethodDto>.CreateError(
                        "This payment method is already configured for the merchant"
                    );
                }

                var merchantPaymentMethod = new MerchantPaymentMethod
                {
                    MerchantId = request.MerchantId,
                    PaymentMethodId = request.PaymentMethodId,
                    Configuration = request.Configuration?.Trim(),
                    IsEnabled = request.IsEnabled,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Set<MerchantPaymentMethod>().Add(merchantPaymentMethod);
                await _context.SaveChangesAsync();

                // Load the full data for response
                var result = await _context.Set<MerchantPaymentMethod>()
                    .Include(mpm => mpm.PaymentMethod)
                    .Where(mpm => mpm.Id == merchantPaymentMethod.Id)
                    .Select(mpm => new MerchantPaymentMethodDto
                    {
                        Id = mpm.Id,
                        MerchantId = mpm.MerchantId,
                        PaymentMethodId = mpm.PaymentMethodId,
                        PaymentMethodName = mpm.PaymentMethod.Name,
                        PaymentMethodDescription = mpm.PaymentMethod.Description,
                        //PaymentMethodImageUrl = mpm.PaymentMethod.ImageUrl,
                        ImageUrl = mpm.PaymentMethod.ImageUrl,
                        Configuration = mpm.Configuration ?? string.Empty,
                        IsEnabled = mpm.IsEnabled,
                        CreatedAt = mpm.CreatedAt,
                        UpdatedAt = mpm.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                _logger.LogInformation("Added payment method {PaymentMethodId} to merchant {MerchantId}", 
                    request.PaymentMethodId, request.MerchantId);

                return PaymentMethodResponse<MerchantPaymentMethodDto>.CreateSuccess(
                    result!,
                    "Payment method added to merchant successfully"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding payment method to merchant");
                return PaymentMethodResponse<MerchantPaymentMethodDto>.CreateError(
                    "Failed to add payment method to merchant"
                );
            }
        }

        public async Task<PaymentMethodResponse<MerchantPaymentMethodDto>> UpdateMerchantPaymentMethodAsync(int id, CreateMerchantPaymentMethodDto request)
        {
            try
            {
                var merchantPaymentMethod = await _context.Set<MerchantPaymentMethod>()
                    .Include(mpm => mpm.PaymentMethod)
                    .FirstOrDefaultAsync(mpm => mpm.Id == id);

                if (merchantPaymentMethod == null)
                {
                    return PaymentMethodResponse<MerchantPaymentMethodDto>.CreateError(
                        "Merchant payment method not found"
                    );
                }

                merchantPaymentMethod.Configuration = request.Configuration?.Trim();
                merchantPaymentMethod.IsEnabled = request.IsEnabled;
                merchantPaymentMethod.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var result = new MerchantPaymentMethodDto
                {
                    Id = merchantPaymentMethod.Id,
                    MerchantId = merchantPaymentMethod.MerchantId,
                    PaymentMethodId = merchantPaymentMethod.PaymentMethodId,
                    PaymentMethodName = merchantPaymentMethod.PaymentMethod.Name,
                    PaymentMethodDescription = merchantPaymentMethod.PaymentMethod.Description,
                    //PaymentMethodImageUrl = merchantPaymentMethod.PaymentMethod.ImageUrl,
                    ImageUrl = merchantPaymentMethod.PaymentMethod.ImageUrl,
                    Configuration = merchantPaymentMethod.Configuration ?? string.Empty,
                    IsEnabled = merchantPaymentMethod.IsEnabled,
                    CreatedAt = merchantPaymentMethod.CreatedAt,
                    UpdatedAt = merchantPaymentMethod.UpdatedAt
                };

                _logger.LogInformation("Updated merchant payment method {Id}", id);

                return PaymentMethodResponse<MerchantPaymentMethodDto>.CreateSuccess(
                    result,
                    "Merchant payment method updated successfully"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating merchant payment method {Id}", id);
                return PaymentMethodResponse<MerchantPaymentMethodDto>.CreateError(
                    "Failed to update merchant payment method"
                );
            }
        }

        public async Task<PaymentMethodResponse<bool>> RemoveMerchantPaymentMethodAsync(int id)
        {
            try
            {
                var merchantPaymentMethod = await _context.Set<MerchantPaymentMethod>()
                    .FindAsync(id);

                if (merchantPaymentMethod == null)
                {
                    return PaymentMethodResponse<bool>.CreateError("Merchant payment method not found");
                }

                _context.Set<MerchantPaymentMethod>().Remove(merchantPaymentMethod);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Removed merchant payment method {Id}", id);

                return PaymentMethodResponse<bool>.CreateSuccess(
                    true,
                    "Payment method removed from merchant successfully"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing merchant payment method {Id}", id);
                return PaymentMethodResponse<bool>.CreateError(
                    "Failed to remove payment method from merchant"
                );
            }
        }

        public async Task<PaymentMethodResponse<bool>> ToggleMerchantPaymentMethodAsync(int id, bool isEnabled)
        {
            try
            {
                var merchantPaymentMethod = await _context.Set<MerchantPaymentMethod>()
                    .FindAsync(id);

                if (merchantPaymentMethod == null)
                {
                    return PaymentMethodResponse<bool>.CreateError("Merchant payment method not found");
                }

                merchantPaymentMethod.IsEnabled = isEnabled;
                merchantPaymentMethod.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Toggled merchant payment method {Id} to {Status}", 
                    id, isEnabled ? "enabled" : "disabled");

                return PaymentMethodResponse<bool>.CreateSuccess(
                    true,
                    $"Payment method {(isEnabled ? "enabled" : "disabled")} successfully"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling merchant payment method {Id}", id);
                return PaymentMethodResponse<bool>.CreateError(
                    "Failed to toggle payment method status"
                );
            }
        }

        #endregion
    }
}