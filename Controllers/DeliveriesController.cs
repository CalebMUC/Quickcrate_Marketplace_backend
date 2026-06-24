using Microsoft.AspNetCore.Mvc;
using Minimart_Api.Services.Deliveries;

namespace Minimart_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveriesController : Controller
    {
        private readonly IDeliveryService _deliveryService;
        public DeliveriesController(IDeliveryService deliveryService) { 
            _deliveryService = deliveryService;
        }

        [HttpGet("counties")]
        public async Task<IActionResult> GetCounties()
        {
            var counties = await _deliveryService.GetCountiesAsync();
            return Ok(counties);
        }

        [HttpGet("towns")]
        public async Task<IActionResult> GetTowns(int countyId)
        {
            var towns = await _deliveryService.GetTownsByCountyAsync(countyId);
            return Ok(towns);
        }

        [HttpGet("deliveryStations")]
        public async Task<IActionResult> GetDeliveryStations(int townId)
        {
            var deliveryStations = await _deliveryService.GetDeliveryStationsByTownAsync(townId);
            return Ok(deliveryStations);
        }
    }
}
