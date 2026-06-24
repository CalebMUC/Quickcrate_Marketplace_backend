namespace Minimart_Api.DTOS.Authorization
{
    public class RegisterResponse
    {
        public int ResponseCode { get; set; }
        public string ResponseMessage { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty; // Identity User ID only
        public string Username { get; set; } = string.Empty;
        public string? Role { get; set; }
    }
}
