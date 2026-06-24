namespace Minimart_Api.DTOS.Security
{
    public class ModuleDto
    {
        public int ModuleID { get; set; }
        public string ModuleName { get; set; }
        public List<SubModuleDto> SubModules { get; set; } = new List<SubModuleDto>();
    }

    public class SubModuleDto
    {
        public int SubModuleID { get; set; }
        public string SubModuleName { get; set; }
        public string SubModuleUrl { get; set; }
        public int Order { get; set; }
    }
}
