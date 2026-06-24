using Minimart_Api.DTOS.Security;

namespace Minimart_Api.Services.SystemSecurity
{
    public interface ISystemSecurity
    {
        Task<List<ModuleDto>> GetRoleModules(string RoleID);

        Task<List<SubModuleCategoriesDto>> GetSubModuleCategories(int subModuleID);

    }
}
