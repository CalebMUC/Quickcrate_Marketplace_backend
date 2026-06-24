using System.Runtime.CompilerServices;
using Minimart_Api.Repositories.Deliveries;
using Minimart_Api.Models;

namespace Minimart_Api.Services.Deliveries
{
    public class DeliveryService : IDeliveryService
    {
        private readonly ILogger _logger;
        private readonly IDeliveriesRepo _deliveryRepo;
        public DeliveryService(IDeliveriesRepo deliveryRepo) { 
            _deliveryRepo = deliveryRepo;
        }
        public async Task<IEnumerable<Counties>> GetCountiesAsync()
        {
            return await _deliveryRepo.GetAllCountiesAsync();
        }

        public async Task<IEnumerable<Towns>> GetTownsByCountyAsync(int countyId)
        {
            return await _deliveryRepo.GetTownsByCountyAsync(countyId);
        }

        public async Task<IEnumerable<DeliveryStations>> GetDeliveryStationsByTownAsync(int townId)
        {
            return await _deliveryRepo.GetDeliveryStationsByTownAsync(townId);
        }
    }
}
