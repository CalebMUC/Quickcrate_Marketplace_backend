using Microsoft.AspNetCore.Mvc;
using Minimart_Api.DTOS.Features;
using Minimart_Api.Services.Features;

namespace Minimart_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeatureController : ControllerBase
    {
        private readonly FeatureService _featureService;
        public FeatureController(FeatureService featureService) {
            _featureService = featureService;
        }

        [HttpPost("TestAdd")]
        public IActionResult TestAdd([FromBody] string testInput)
        {
            return Ok($"Received: {testInput}");
        }

        [HttpPost("AddFeaturesNew")]
        public async Task<IActionResult> AddFeatures([FromBody] FeatureDTO request)
        {
            try
            {

                var Response = await _featureService.AddFeatures(request);

                return Ok(Response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
