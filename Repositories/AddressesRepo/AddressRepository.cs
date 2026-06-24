using Microsoft.EntityFrameworkCore;
using Minimart_Api.Data;
using Minimart_Api.DTOS.Address;
using Minimart_Api.DTOS.Notification;
using Minimart_Api.Models;
using Minimart_Api.Repositories.AddressesRepo;

namespace Minimart_Api.Repositories.AddressesRepo
{
    /// <summary>
    /// Enhanced address repository with modern patterns and comprehensive functionality
    /// </summary>
    public class AddressRepository : IAddressRepository
    {
        private readonly MinimartDBContext _dbContext;
        private readonly ILogger<AddressRepository> _logger;

        public AddressRepository(
            MinimartDBContext dBContext, 
            ILogger<AddressRepository> logger)
        {
            _dbContext = dBContext;
            _logger = logger;
        }

        #region Modern CRUD Operations

        public async Task<Addresses?> GetAddressByIdAsync(int addressId)
        {
            try
            {
                return await _dbContext.Addresses
                    .FirstOrDefaultAsync(a => a.AddressID == addressId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving address {AddressId}", addressId);
                throw;
            }
        }

        public async Task<IEnumerable<Addresses>> GetAddressesByUserIdAsync(string userId)
        {
            try
            {
                return await _dbContext.Addresses
                    .Where(a => a.ApplicationUserId == userId)
                    .OrderByDescending(a => a.isDefault)
                    .ThenByDescending(a => a.CreatedOn)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving addresses for user {UserId}", userId);
                throw;
            }
        }

        public async Task<OperationResult> AddAddressAsync(AddressDTO address)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    // Validate user exists
                    var userExists = await _dbContext.Users
                        .AnyAsync(u => u.Id == address.ApplicationUserId);
                    
                    if (!userExists)
                    {
                        return OperationResult.Failure("User not found");
                    }

                    // Handle default address logic
                    if (address.isDefault)
                    {
                        await ResetExistingDefaultAddressAsync(address.ApplicationUserId!);
                    }

                    var newAddress = new Addresses
                    {
                        ApplicationUserId = address.ApplicationUserId,
                        Name = address.Name?.Trim() ?? string.Empty,
                        Phonenumber = address.Phonenumber?.Trim() ?? string.Empty,
                        PostalAddress = address.PostalAddress?.Trim() ?? string.Empty,
                        County = address.County?.Trim() ?? string.Empty,
                        Town = address.Town?.Trim() ?? string.Empty,
                        PostalCode = address.PostalCode?.Trim() ?? string.Empty,
                        ExtraInformation = address.ExtraInformation?.Trim() ?? string.Empty,
                        isDefault = address.isDefault,
                        CreatedOn = DateTime.UtcNow,
                        LastUpdatedOn = DateTime.UtcNow
                    };

                    await _dbContext.Addresses.AddAsync(newAddress);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Address {AddressId} created for user {UserId}", 
                        newAddress.AddressID, address.ApplicationUserId);

                    return OperationResult.Success(newAddress.AddressID, "Address created successfully");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error adding address to database for user {UserId}", 
                        address.ApplicationUserId);
                    throw;
                }
            });
        }

        public async Task<OperationResult> UpdateAddressAsync(EditAddressDTO address)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var existingAddress = await _dbContext.Addresses
                        .FirstOrDefaultAsync(a => a.AddressID == address.AddressID);

                    if (existingAddress == null)
                    {
                        return OperationResult.Failure("Address not found");
                    }

                    // Verify ownership
                    if (existingAddress.ApplicationUserId != address.ApplicationUserId)
                    {
                        return OperationResult.Failure("Address does not belong to the user");
                    }

                    // Handle default address logic
                    if (address.isDefault && !existingAddress.isDefault)
                    {
                        await ResetExistingDefaultAddressAsync(address.ApplicationUserId!, address.AddressID);
                    }

                    // Update fields
                    existingAddress.Name = address.Name?.Trim() ?? string.Empty;
                    existingAddress.Phonenumber = address.Phonenumber?.Trim() ?? string.Empty;
                    existingAddress.PostalAddress = address.PostalAddress?.Trim() ?? string.Empty;
                    existingAddress.County = address.County?.Trim() ?? string.Empty;
                    existingAddress.Town = address.Town?.Trim() ?? string.Empty;
                    existingAddress.PostalCode = address.PostalCode?.Trim() ?? string.Empty;
                    existingAddress.ExtraInformation = address.ExtraInformation?.Trim() ?? string.Empty;
                    existingAddress.isDefault = address.isDefault;
                    existingAddress.LastUpdatedOn = DateTime.UtcNow;

                    _dbContext.Addresses.Update(existingAddress);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Address {AddressId} updated for user {UserId}", 
                        address.AddressID, address.ApplicationUserId);

                    return OperationResult.Success("Address updated successfully");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error updating address {AddressId}", address.AddressID);
                    throw;
                }
            });
        }

        public async Task<OperationResult> DeleteAddressAsync(int addressId)
        {
            try
            {
                var address = await _dbContext.Addresses
                    .FirstOrDefaultAsync(a => a.AddressID == addressId);

                if (address == null)
                {
                    return OperationResult.Failure("Address not found");
                }

                // If this was the default address, we might want to set another as default
                if (address.isDefault)
                {
                    var otherAddress = await _dbContext.Addresses
                        .Where(a => a.ApplicationUserId == address.ApplicationUserId && 
                                   a.AddressID != addressId)
                        .OrderByDescending(a => a.CreatedOn)
                        .FirstOrDefaultAsync();

                    if (otherAddress != null)
                    {
                        otherAddress.isDefault = true;
                        _dbContext.Addresses.Update(otherAddress);
                    }
                }

                _dbContext.Addresses.Remove(address);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Address {AddressId} deleted for user {UserId}", 
                    addressId, address.ApplicationUserId);

                return OperationResult.Success("Address deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address {AddressId}", addressId);
                throw;
            }
        }

        #endregion

        #region Address Management Operations

        public async Task<OperationResult> SetDefaultAddressAsync(int addressId, string userId)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var address = await _dbContext.Addresses
                        .FirstOrDefaultAsync(a => a.AddressID == addressId && a.ApplicationUserId == userId);

                    if (address == null)
                    {
                        return OperationResult.Failure("Address not found or does not belong to user");
                    }

                    if (!address.isDefault)
                    {
                        // Reset other default addresses
                        await ResetExistingDefaultAddressAsync(userId, addressId);

                        // Set this address as default
                        address.isDefault = true;
                        address.LastUpdatedOn = DateTime.UtcNow;
                        
                        _dbContext.Addresses.Update(address);
                        await _dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();

                        _logger.LogInformation("Address {AddressId} set as default for user {UserId}", 
                            addressId, userId);
                    }

                    return OperationResult.Success("Default address updated successfully");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error setting default address {AddressId} for user {UserId}", 
                        addressId, userId);
                    throw;
                }
            });
        }

        public async Task<Addresses?> GetDefaultAddressAsync(string userId)
        {
            try
            {
                return await _dbContext.Addresses
                    .FirstOrDefaultAsync(a => a.ApplicationUserId == userId && a.isDefault);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving default address for user {UserId}", userId);
                throw;
            }
        }

        public async Task<int> GetAddressCountAsync(string userId)
        {
            try
            {
                return await _dbContext.Addresses
                    .CountAsync(a => a.ApplicationUserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting address count for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> AddressExistsAsync(int addressId, string userId)
        {
            try
            {
                return await _dbContext.Addresses
                    .AnyAsync(a => a.AddressID == addressId && a.ApplicationUserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking address existence {AddressId} for user {UserId}", 
                    addressId, userId);
                throw;
            }
        }

        #endregion

        #region Legacy Support

        public async Task<Addresses?> GetAddressByIdLegacyAsync(int addressId)
        {
            return await GetAddressByIdAsync(addressId);
        }

        public async Task<IEnumerable<GetAddressDTO>> GetAddressesByUserIdLegacyAsync(string userId)
        {
            try
            {
                return await _dbContext.Addresses
                    .Where(a => a.ApplicationUserId == userId)
                    .GroupJoin(_dbContext.Counties,
                        address => address.County,
                        county => county.CountyName,
                        (address, counties) => new { Address = address, Counties = counties })
                    .SelectMany(
                        x => x.Counties.DefaultIfEmpty(),
                        (x, county) => new { x.Address, County = county })
                    .GroupJoin(_dbContext.Towns,
                        combined => combined.Address.Town,
                        town => town.TownName,
                        (combined, towns) => new { combined.Address, combined.County, Towns = towns })
                    .SelectMany(
                        x => x.Towns.DefaultIfEmpty(),
                        (x, town) => new GetAddressDTO
                        {
                            AddressID = x.Address.AddressID,
                            ApplicationUserId = x.Address.ApplicationUserId,
                            Name = x.Address.Name,
                            PhoneNumber = x.Address.Phonenumber,
                            PostalAddress = x.Address.PostalAddress,
                            County = x.Address.County,
                            CountyId = x.County != null ? x.County.CountyId : 0,
                            Town = x.Address.Town,
                            TownId = town != null ? town.TownId : 0,
                            PostalCode = x.Address.PostalCode,
                            ExtraInformation = x.Address.ExtraInformation,
                            isDefault = x.Address.isDefault
                        })
                    .OrderByDescending(a => a.isDefault)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in legacy get addresses method for user {UserId}", userId);
                throw;
            }
        }

        public async Task<OperationResult> AddAddressLegacyAsync(AddressDTO address)
        {
            return await AddAddressAsync(address);
        }

        public async Task<OperationResult> UpdateAddressLegacyAsync(EditAddressDTO address)
        {
            return await UpdateAddressAsync(address);
        }

        #endregion

        #region Legacy Interface Implementation

        async Task<Addresses> IAddressRepo.GetAddressByIdAsync(int addressId)
        {
            return await GetAddressByIdAsync(addressId) ?? new Addresses();
        }

        async Task<IEnumerable<GetAddressDTO>> IAddressRepo.GetAddressesByUserIdAsync(string userId)
        {
            return await GetAddressesByUserIdLegacyAsync(userId);
        }

        async Task<OperationResult> IAddressRepo.AddAddressAsync(AddressDTO address)
        {
            return await AddAddressAsync(address);
        }

        async Task<OperationResult> IAddressRepo.EditAddressAsync(EditAddressDTO address)
        {
            return await UpdateAddressAsync(address);
        }

        #endregion

        #region Helper Methods

        private async Task ResetExistingDefaultAddressAsync(string applicationUserId, int? excludeAddressId = null)
        {
            try
            {
                var query = _dbContext.Addresses
                    .Where(a => a.ApplicationUserId == applicationUserId && a.isDefault);

                if (excludeAddressId.HasValue)
                {
                    query = query.Where(a => a.AddressID != excludeAddressId.Value);
                }

                var defaultAddresses = await query.ToListAsync();

                foreach (var addr in defaultAddresses)
                {
                    addr.isDefault = false;
                    addr.LastUpdatedOn = DateTime.UtcNow;
                }

                if (defaultAddresses.Any())
                {
                    _dbContext.Addresses.UpdateRange(defaultAddresses);
                    await _dbContext.SaveChangesAsync();
                    
                    _logger.LogInformation("Reset {Count} default addresses for user {UserId}", 
                        defaultAddresses.Count, applicationUserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting default addresses for user {UserId}", applicationUserId);
                throw;
            }
        }

        #endregion
    }
}