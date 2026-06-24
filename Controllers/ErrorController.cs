using Microsoft.AspNetCore.Mvc;

namespace Minimart_Api.Controllers
{
    [ApiController]
    public class ErrorController : ControllerBase
    {
        [Route("/error")]
        [HttpGet]
        public IActionResult HandleError() => Problem();
       
    }
}
