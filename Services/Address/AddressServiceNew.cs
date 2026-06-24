using Minimart_Api.DTOS.Address;
using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.Notification;
using Minimart_Api.Models;
using Minimart_Api.Repositories.AddressesRepo;
using Minimart_Api.Controllers;

namespace Minimart_Api.Services.Address
{
    /// <summary>
    /// Enhanced address service with modern patterns and comprehensive functionality
    /// </summary>
    public class AddressServiceNew : IAddressService
    {
        private readonly IAddressRepository _addressRepository;
        private readonly ILogger<AddressServiceNew> _logger;

        public AddressServiceNew(
            IAddressRepository addressRepository,
            ILogger<AddressServiceNew> logger)
        {
            _addressRepository = addressRepository;
            _logger = logger;
        }

        #region Legacy Support - IAddress Implementation

        async Task<Addresses> IAddress.GetAddressByIdAsync(int addressId)
        {
            return await _addressRepository.GetAddressByIdLegacyAsync(addressId) ?? new Addresses();
        }

        async Task<IEnumerable<GetAddressDTO>> IAddress.GetAddressesByUserIdAsync(string userId)
        {
            return await _addressRepository.GetAddressesByUserIdLegacyAsync(userId);
        }

        async Task<OperationResult> IAddress.AddAddressAsync(AddressDTO address)
        {
            return await _addressRepository.AddAddressLegacyAsync(address);
        }

        async Task<OperationResult> IAddress.EditAddressAsync(EditAddressDTO address)
        {
            return await _addressRepository.UpdateAddressLegacyAsync(address);
        }

        #endregion

        #region Modern CRUD Operations

        public async Task<ApiResponse<AddressDetailDto>> GetAddressByIdAsync(int addressId)
        {
            try
            {
                if (addressId <= 0)
                {
                    return ApiResponse<AddressDetailDto>.CreateError("Invalid address ID");
                }

                var address = await _addressRepository.GetAddressByIdAsync(addressId);
                
                if (address == null)
                {
                    return ApiResponse<AddressDetailDto>.CreateError("Address not found");
                }

                var addressDto = MapToDetailDto(address);
                return ApiResponse<AddressDetailDto>.CreateSuccess(addressDto, "Address retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving address {AddressId}", addressId);
                return ApiResponse<AddressDetailDto>.CreateError("Failed to retrieve address");
            }
        }

        public async Task<ApiResponse<IEnumerable<AddressDetailDto>>> GetAddressesByUserIdAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return ApiResponse<IEnumerable<AddressDetailDto>>.CreateError("User ID is required");
                }

                var addresses = await _addressRepository.GetAddressesByUserIdAsync(userId);
                var addressDtos = addresses.Select(MapToDetailDto);
                
                return ApiResponse<IEnumerable<AddressDetailDto>>.CreateSuccess(
                    addressDtos, 
                    $"Retrieved {addressDtos.Count()} addresses");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving addresses for user {UserId}", userId);
                return ApiResponse<IEnumerable<AddressDetailDto>>.CreateError("Failed to retrieve addresses");
            }
        }

        public async Task<ApiResponse<int>> AddAddressAsync(CreateAddressRequest request)
        {
            try
            {
                if (request == null)
                {
                    return ApiResponse<int>.CreateError("Request cannot be null");
                }

                var address = MapFromCreateRequest(request);
                var result = await _addressRepository.AddAddressAsync(address);
                
                if (!result.IsSuccess)
                {
                    return ApiResponse<int>.CreateError(result.Message);
                }

                var addressId = (int)result.Data!;
                
                return ApiResponse<int>.CreateSuccess(addressId, "Address created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address for user {UserId}", request?.ApplicationUserId);
                return ApiResponse<int>.CreateError("Failed to create address");
            }
        }

        public async Task<ApiResponse<bool>> UpdateAddressAsync(UpdateAddressRequest request)
        {
            try
            {
                if (request == null || request.AddressID <= 0)
                {
                    return ApiResponse<bool>.CreateError("Invalid request or address ID");
                }

                var editDto = MapFromUpdateRequest(request);
                var result = await _addressRepository.EditAddressAsync(editDto);
                
                if (!result.IsSuccess)
                {
                    return ApiResponse<bool>.CreateError(result.Message);
                }
                
                return ApiResponse<bool>.CreateSuccess(true, "Address updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address {AddressId}", request?.AddressID);
                return ApiResponse<bool>.CreateError("Failed to update address");
            }
        }

        public async Task<ApiResponse<bool>> DeleteAddressAsync(int addressId)
        {
            try
            {
                if (addressId <= 0)
                {
                    return ApiResponse<bool>.CreateError("Invalid address ID");
                }

                var result = await _addressRepository.DeleteAddressAsync(addressId);
                
                if (!result.IsSuccess)
                {
                    return ApiResponse<bool>.CreateError(result.Message);
                }
                
                return ApiResponse<bool>.CreateSuccess(true, "Address deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address {AddressId}", addressId);
                return ApiResponse<bool>.CreateError("Failed to delete address");
            }
        }

        #endregion

        #region Address Management

        public async Task<ApiResponse<bool>> SetDefaultAddressAsync(int addressId, string userId)
        {
            try
            {
                if (addressId <= 0)
                {
                    return ApiResponse<bool>.CreateError("Invalid address ID");
                }

                var result = await _addressRepository.SetDefaultAddressAsync(addressId, userId);
                
                if (!result.IsSuccess)
                {
                    return ApiResponse<bool>.CreateError(result.Message);
                }
                
                return ApiResponse<bool>.CreateSuccess(true, "Default address updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default address {AddressId}", addressId);
                return ApiResponse<bool>.CreateError("Failed to set default address");
            }
        }

        public async Task<ApiResponse<AddressDetailDto>> GetDefaultAddressAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return ApiResponse<AddressDetailDto>.CreateError("User ID is required");
                }

                var address = await _addressRepository.GetDefaultAddressAsync(userId);
                
                if (address == null)
                {
                    return ApiResponse<AddressDetailDto>.CreateError("No default address found");
                }

                var addressDto = MapToDetailDto(address);
                return ApiResponse<AddressDetailDto>.CreateSuccess(addressDto, 
                    "Default address retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving default address for user {UserId}", userId);
                return ApiResponse<AddressDetailDto>.CreateError("Failed to retrieve default address");
            }
        }

        public async Task<ApiResponse<AddressValidationResult>> ValidateAddressAsync(CreateAddressRequest request)
        {
            try
            {
                var result = new AddressValidationResult { IsValid = true };

                if (request == null)
                {
                    result.AddError("Request cannot be null");
                    return ApiResponse<AddressValidationResult>.CreateSuccess(result);
                }

                if (string.IsNullOrWhiteSpace(request.Name))
                    result.AddError("Name is required");

                if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                    result.AddError("Phone number is required");

                if (string.IsNullOrWhiteSpace(request.PostalAddress))
                    result.AddError("Postal address is required");

                if (string.IsNullOrWhiteSpace(request.County))
                    result.AddError("County is required");

                if (string.IsNullOrWhiteSpace(request.Town))
                    result.AddError("Town is required");

                return ApiResponse<AddressValidationResult>.CreateSuccess(result, "Address validation completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating address");
                return ApiResponse<AddressValidationResult>.CreateError("Failed to validate address");
            }
        }

        public async Task<ApiResponse<int>> GetAddressCountAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return ApiResponse<int>.CreateError("User ID is required");
                }

                var count = await _addressRepository.GetAddressCountAsync(userId);
                return ApiResponse<int>.CreateSuccess(count, $"User has {count} addresses");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting address count for user {UserId}", userId);
                return ApiResponse<int>.CreateError("Failed to get address count");
            }
        }

        #endregion

        #region Helper Methods

        private static AddressDetailDto MapToDetailDto(Addresses address)
        {
            return new AddressDetailDto
            {
                AddressID = address.AddressID,
                ApplicationUserId = address.ApplicationUserId,
                Name = address.Name,
                PhoneNumber = address.Phonenumber,
                PostalAddress = address.PostalAddress,
                County = address.County,
                Town = address.Town,
                PostalCode = address.PostalCode,
                ExtraInformation = address.ExtraInformation,
                IsDefault = address.isDefault,
                CreatedOn = address.CreatedOn,
                LastUpdatedOn = address.LastUpdatedOn
            };
        }

        private static AddressDTO MapFromCreateRequest(CreateAddressRequest request)
        {
            return new AddressDTO
            {
                ApplicationUserId = request.ApplicationUserId,
                Name = request.Name,
                Phonenumber = request.PhoneNumber,
                PostalAddress = request.PostalAddress,
                County = request.County,
                Town = request.Town,
                PostalCode = request.PostalCode,
                ExtraInformation = request.ExtraInformation,
                isDefault = request.IsDefault
            };
        }

        private static EditAddressDTO MapFromUpdateRequest(UpdateAddressRequest request)
        {
            return new EditAddressDTO
            {
                AddressID = request.AddressID,
                ApplicationUserId = request.ApplicationUserId,
                Name = request.Name,
                Phonenumber = request.PhoneNumber,
                PostalAddress = request.PostalAddress,
                County = request.County,
                Town = request.Town,
                PostalCode = request.PostalCode,
                ExtraInformation = request.ExtraInformation,
                isDefault = request.IsDefault
            };
        }

        #endregion
    }
}