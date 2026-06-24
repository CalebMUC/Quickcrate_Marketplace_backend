using System.Text.Json.Serialization;

namespace Minimart_Api.DTOS.General
{
    /// <summary>
    /// Standardized API response wrapper for all dashboard endpoints
    /// </summary>
    /// <typeparam name="T">Type of data being returned</typeparam>
    public class DashboardResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public T? Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("errors")]
        public List<string> Errors { get; set; } = new();

        public static DashboardResponse<T> CreateSuccess(T data, string message = "Data retrieved successfully")
        {
            return new DashboardResponse<T>
            {
                Success = true,
                Data = data,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }

        public static DashboardResponse<T> CreateError(string message, List<string>? errors = null)
        {
            return new DashboardResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>(),
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Non-generic dashboard response for operations without data
    /// </summary>
    public class DashboardResponse : DashboardResponse<object>
    {
        public static DashboardResponse CreateSuccess(string message = "Operation completed successfully")
        {
            return new DashboardResponse
            {
                Success = true,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }

        public static new DashboardResponse CreateError(string message, List<string>? errors = null)
        {
            return new DashboardResponse
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>(),
                Timestamp = DateTime.UtcNow
            };
        }
    }
}