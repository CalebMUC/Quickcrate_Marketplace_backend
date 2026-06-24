using Microsoft.AspNetCore.Mvc;
using Minimart_Api.Services.SystemSecurity;
//using OpenSearch.Client;

namespace Minimart_Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class SystemSecurityController : ControllerBase
    {
        private readonly ISystemSecurity _systemSecurity;

        public SystemSecurityController(ISystemSecurity systemSecurity) { 
            _systemSecurity = systemSecurity;
        }
        [HttpGet("submodule-categories")]
        public async Task<IActionResult> GetSubModuleCategories(int subModuleID) {
            try
            {
                var response =  await _systemSecurity.GetSubModuleCategories(subModuleID);

                return Ok(response);
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("role-modules")]
        public async Task<IActionResult> GetModules(string RoleID)
        {
            try {
                if (string.IsNullOrEmpty(RoleID)) {
                    return BadRequest("Role cannot be null, please provide role");
                }
                var response  = await _systemSecurity.GetRoleModules(RoleID);


                return Ok(response);
            }catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
