using System.Data;
using Microsoft.Data.SqlClient;
using Minimart_Api.Data;
using Minimart_Api.Models;

namespace Minimart_Api.Repositories.Reports
{
    public class ReportRepo : IReportRepo
    {
        private readonly MinimartDBContext _dbContext;
        private readonly IConfiguration _configuration;
        public ReportRepo(MinimartDBContext dBContext, IConfiguration configuration)
        {

            _dbContext = dBContext;
            _configuration = configuration;
        }





        public async Task<DataSet> GetReportData(string reportType, Dictionary<string, string> parameters)
        {
            // Create a new DataSet to store the result
            DataSet dataSet = new DataSet();

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand($"Get{reportType.Replace(" ", "")}", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters to the command
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue($"@{param.Key}", param.Value);
                    }

                    // Use SqlDataAdapter to fill the DataSet
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dataSet); // Fills the DataSet with the result of the query
                    }
                }
            }

            return dataSet;
        }
    }
}

