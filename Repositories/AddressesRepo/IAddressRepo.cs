using Minimart_Api.DTOS.Address;
using Minimart_Api.DTOS.Notification;
using Minimart_Api.Models;


namespace Minimart_Api.Repositories.AddressesRepo
{
    public interface IAddressRepo
    {
        Task<Addresses> GetAddressByIdAsync(int addressId);
        Task<IEnumerable<GetAddressDTO>> GetAddressesByUserIdAsync(string userId);
        Task<OperationResult> AddAddressAsync(AddressDTO address);
        Task<OperationResult> EditAddressAsync(EditAddressDTO address);

        //Task SaveChangesAsync();
    }
}
