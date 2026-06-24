using Minimart_Api.Models;

namespace Minimart_Api.Services.Deliveries
{
    public interface IDeliveryService
    {
        Task<IEnumerable<Counties>> GetCountiesAsync();
        Task<IEnumerable<Towns>> GetTownsByCountyAsync(int countyId);
        Task<IEnumerable<DeliveryStations>> GetDeliveryStationsByTownAsync(int townId);
    }
}
