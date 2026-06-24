using System.Data;
using Minimart_Api.Repositories.Reports;
using Minimart_Api.Services.ReportService.ReportService;

namespace Minimart_Api.Services.ReportService
{
    public class ReportServices : IReportService
    {
        private readonly IReportRepo _reportRepo;
        public ReportServices(IReportRepo reportRepo)
        {
            _reportRepo = reportRepo;
        }
        public async Task<DataSet> GetReportData(string reportType, Dictionary<string, string> parameters)
        {

            var ReportData = await _reportRepo.GetReportData(reportType, parameters);

            return ReportData;
        }
    }
}
