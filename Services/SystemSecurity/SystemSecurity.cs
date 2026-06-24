using Minimart_Api.DTOS.Security;
using Minimart_Api.Repositories.SystemSecurityRepo;

namespace Minimart_Api.Services.SystemSecurity
{

    public class SystemSecurity : ISystemSecurity
    {
        private readonly ISystemSecurityRepo _repo;
        public SystemSecurity(ISystemSecurityRepo securityRepo) { 
            _repo = securityRepo;
        }

        public async Task<List<ModuleDto>> GetRoleModules(string RoleID) {
            return await _repo.GetRoleModules(RoleID);
        }

        public async Task<List<SubModuleCategoriesDto>> GetSubModuleCategories(int subModuleID)
        {
            return await _repo.GetSubModuleCategories(subModuleID);
        }
    }
}
