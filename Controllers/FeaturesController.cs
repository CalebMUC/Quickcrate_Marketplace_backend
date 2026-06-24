using Microsoft.AspNetCore.Mvc;
using Minimart_Api.DTOS.Features;
using Minimart_Api.Services.Features;

namespace Minimart_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeaturesController : ControllerBase
    {
        private readonly IFeatureService _featureService;
        public FeaturesController(IFeatureService featureService) { 
            _featureService = featureService;
        }

        [HttpPost("TestAdd")]
        public IActionResult TestAdd([FromBody] string testInput)
        {
            return Ok($"Received: {testInput}");
        }

        [HttpPost("AddFeatures")]
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

        [HttpGet("GetAllFeatures")]
        public async Task<IActionResult> GetAllFeatures()
        {
            try
            {
                var response = await _featureService.GetAllFeatures();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/SubcategoryFeature/GetFeatures/{subcategoryId}
        [HttpPost("GetFeatures")]
        public async Task<IActionResult> GetFeaturesForSubcategory(FeatureRequestDTO feature)
        {
            var features = await _featureService.GetFeatures(feature);

            if (features == null || !features.Any())
            {
                //return NotFound("No features found for this subcategory.");

                return Ok(features);
            }

            return Ok(features);
        }
    }
}
