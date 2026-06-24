using Minimart_Api.DTOS.Address;
using Minimart_Api.DTOS.General;
using Minimart_Api.Models;

namespace Minimart_Api.Services.Address
{
    /// <summary>
    /// Enhanced Address service interface following modern patterns
    /// </summary>
    public interface IAddressService : IAddress
    {
        #region Modern CRUD Operations
        
        /// <summary>
        /// Get address by ID with proper error handling
        /// </summary>
        new Task<ApiResponse<AddressDetailDto>> GetAddressByIdAsync(int addressId);
        
        /// <summary>
        /// Get all addresses for a specific user
        /// </summary>
        new Task<ApiResponse<IEnumerable<AddressDetailDto>>> GetAddressesByUserIdAsync(string userId);
        
        /// <summary>
        /// Add new address with validation
        /// </summary>
        Task<ApiResponse<int>> AddAddressAsync(Controllers.CreateAddressRequest request);
        
        /// <summary>
        /// Update existing address
        /// </summary>
        Task<ApiResponse<bool>> UpdateAddressAsync(Controllers.UpdateAddressRequest request);
        
        /// <summary>
        /// Soft delete address
        /// </summary>
        Task<ApiResponse<bool>> DeleteAddressAsync(int addressId);

        #endregion

        #region Address Management

        /// <summary>
        /// Set address as default for user
        /// </summary>
        Task<ApiResponse<bool>> SetDefaultAddressAsync(int addressId, string userId);
        
        /// <summary>
        /// Get user's default address
        /// </summary>
        Task<ApiResponse<AddressDetailDto>> GetDefaultAddressAsync(string userId);
        
        /// <summary>
        /// Validate address data and format
        /// </summary>
        Task<ApiResponse<AddressValidationResult>> ValidateAddressAsync(Controllers.CreateAddressRequest request);
        
        /// <summary>
        /// Get address count for user
        /// </summary>
        Task<ApiResponse<int>> GetAddressCountAsync(string userId);

        #endregion
    }
}