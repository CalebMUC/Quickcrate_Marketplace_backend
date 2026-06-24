using Minimart_Api.Models;

namespace Minimart_Api.Repositories.Deliveries
{
    public interface IDeliveriesRepo
    {

        Task<IEnumerable<Counties>> GetAllCountiesAsync();
        Task<IEnumerable<Towns>> GetTownsByCountyAsync(int countyId);
        Task<IEnumerable<DeliveryStations>> GetDeliveryStationsByTownAsync(int townId);
    }
}
