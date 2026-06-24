using Minimart_Api.DTOS.Address;
using Minimart_Api.DTOS.Notification;
using Minimart_Api.Models;

namespace Minimart_Api.Repositories.AddressesRepo
{
    /// <summary>
    /// Enhanced address repository interface with modern patterns
    /// </summary>
    public interface IAddressRepository : IAddressRepo
    {
        #region Modern CRUD Operations
        
        /// <summary>
        /// Get all addresses for a user with complete data
        /// </summary>
        new Task<IEnumerable<Addresses>> GetAddressesByUserIdAsync(string userId);
        
        /// <summary>
        /// Soft delete address
        /// </summary>
        Task<OperationResult> DeleteAddressAsync(int addressId);

        #endregion

        #region Address Management Operations
        
        /// <summary>
        /// Set address as default for user
        /// </summary>
        Task<OperationResult> SetDefaultAddressAsync(int addressId, string userId);
        
        /// <summary>
        /// Get user's default address
        /// </summary>
        Task<Addresses?> GetDefaultAddressAsync(string userId);
        
        /// <summary>
        /// Get address count for user
        /// </summary>
        Task<int> GetAddressCountAsync(string userId);
        
        /// <summary>
        /// Check if address exists for user
        /// </summary>
        Task<bool> AddressExistsAsync(int addressId, string userId);

        #endregion

        #region Legacy Support
        
        /// <summary>
        /// Legacy method for backward compatibility
        /// </summary>
        Task<Addresses?> GetAddressByIdLegacyAsync(int addressId);
        
        /// <summary>
        /// Legacy method for backward compatibility
        /// </summary>
        Task<IEnumerable<GetAddressDTO>> GetAddressesByUserIdLegacyAsync(string userId);
        
        /// <summary>
        /// Legacy method for backward compatibility
        /// </summary>
        Task<OperationResult> AddAddressLegacyAsync(AddressDTO address);
        
        /// <summary>
        /// Legacy method for backward compatibility
        /// </summary>
        Task<OperationResult> UpdateAddressLegacyAsync(EditAddressDTO address);

        #endregion
    }
}