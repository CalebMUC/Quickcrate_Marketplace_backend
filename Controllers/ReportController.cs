using Microsoft.AspNetCore.Mvc;
using Minimart_Api.Mappings;
using System.Data;
//using Microsoft.Reporting.WebForms;
using AspNetCore.Reporting;
using System.Net.Http.Headers;
using System.Net;
using Minimart_Api.DTOS.Reports;
using Minimart_Api.Services.ReportService.ReportService;

//using Minimart_Api.;

namespace Minimart_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost("Generate")]
        public async Task<IActionResult> GenerateReport([FromBody] ReportRequestData requestData)
        {
            try
            {
                if (!ValidateParameters(requestData.ReportType, requestData.parameters)) {
                    return BadRequest("inValid Parameters");
                }

                var reportData = await _reportService.GetReportData(requestData.ReportType, requestData.parameters);

                var response = ConvertDataTableToList(reportData.Tables[0]);

                return Ok(response);  

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }




        [HttpPost("Export")]
        public async Task<IActionResult> Export([FromBody] ReportRequestData requestData)
        {
            try
            {
                // Construct the SSRS report URL for rendering
                var reportServerUrl = "http://desktop-oe12rm4/ReportServer"; // Use ReportServer, not Reports
                var reportPath = $"/ALLREPORTS/{Uri.EscapeDataString(requestData.ReportType.Replace(" ", ""))}";
                var format = requestData.Format.ToUpper();

                var parameters = string.Join("&", requestData.parameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
                var reportUrl = $"{reportServerUrl}?{reportPath}&rs:Command=Render&rs:Format={format}&{parameters}";

                // Configure HttpClientHandler for Windows Authentication
                var handler = new HttpClientHandler
                {
                    Credentials = new NetworkCredential("ADMIN", "dennis@2543#", "DESKTOP-OE12RM4"),
                    UseDefaultCredentials = false
                };

                using var httpClient = new HttpClient(handler);
                 
                // Fetch the report from the SSRS server
                var response = await httpClient.GetAsync(reportUrl);

                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest($"Failed to fetch report: {response.StatusCode} - {response.ReasonPhrase}");
                }

                // Validate and read the response content
                var reportData = await response.Content.ReadAsByteArrayAsync();

                if (reportData == null || reportData.Length == 0)
                {
                    return BadRequest("The report content is empty.");
                }

                // Check if the response is potentially an HTML error page
                var contentTypeFromResponse = response.Content.Headers.ContentType?.MediaType;
                if (contentTypeFromResponse != null && contentTypeFromResponse.Contains("text/html"))
                {
                    var errorMessage = System.Text.Encoding.UTF8.GetString(reportData);
                    return BadRequest($"The server returned an error: {errorMessage}");
                }

                // Set the content type and file extension based on the requested format
                var contentType = format switch
                {
                    "PDF" => "application/pdf",
                    "EXCELOPENXML" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "WORDOPENXML" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    _ => throw new ArgumentException($"Unsupported format: {format}")
                };

                var fileExtension = format switch
                {
                    "PDF" => "pdf",
                    "EXCELOPENXML" => "xlsx",
                    "WORDOPENXML" => "docx",
                    _ => throw new ArgumentException($"Unsupported format: {format}")
                };

                // Return the report file to the client
                var fileName = $"{requestData.ReportType.Replace(" ", "")}.{fileExtension}";
                return File(reportData, contentType, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }






        private bool ValidateParameters(string reportType, Dictionary<string, string> parameters)
        {
            if (!OrderMapper.ReportConfiguration.ReportParameterMappings.TryGetValue(reportType, out var requiredParameters))
                return false;

            foreach (var requiredParameter in requiredParameters)
            {
                if (!parameters.ContainsKey(requiredParameter) || string.IsNullOrWhiteSpace(parameters[requiredParameter]))
                {
                    return false;
                }
            }
            return true;

        }

        private List<Dictionary<string, object>> ConvertDataTableToList(DataTable table)
        {
            var result = new List<Dictionary<string, object>>();

            foreach (DataRow row in table.Rows)
            {
                var rowDict = new Dictionary<string, object>();
                foreach (DataColumn column in table.Columns)
                {
                    rowDict[column.ColumnName] = row[column];
                }
                result.Add(rowDict);
            }

            return result;
        }
    }
}

