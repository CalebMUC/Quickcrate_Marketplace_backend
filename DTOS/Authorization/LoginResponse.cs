namespace Minimart_Api.DTOS.Authorization
{
    public class LoginResponse
    {
        public string UserId { get; set; } = string.Empty; // Identity User ID (string)
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;

        public string Refreshtoken { get; set; } = string.Empty;
        public int ResponseCode { get; set; }
        public string ResponseMessage { get; set; } = string.Empty;
    }
}
