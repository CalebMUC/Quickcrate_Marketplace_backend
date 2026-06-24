namespace Minimart_Api.DTOS.Authorization
{
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AuthData? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class AuthData
    {
        public UserResponse User { get; set; } = null!;
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public bool RequiresPasswordReset { get; set; }
    }

    public class UserResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid? MerchantId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public bool IsTemporaryPassword { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool EmailVerified { get; set; }
    }
}
