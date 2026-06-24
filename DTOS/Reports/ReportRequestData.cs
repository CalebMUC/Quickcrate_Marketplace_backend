namespace Minimart_Api.DTOS.Reports
{
    public class ReportRequestData
    {
        public string ReportType { get; set; }
        public string Format { get; set; }

        public Dictionary<string, string> parameters { get; set; }

    }
}
