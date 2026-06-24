using System.Data;

namespace Minimart_Api.Services.ReportService.ReportService
{
    public interface IReportService
    {
        public Task<DataSet> GetReportData(string reportType, Dictionary<string, string> parameters);


    }
}
