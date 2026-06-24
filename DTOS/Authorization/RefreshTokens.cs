namespace Minimart_Api.DTOS.Authorization
{
    public class RefreshTokens
    {
        public string RefreshToken { get; set; }

        public string UserName { get; set; }

        public DateTime ExpiryDate { get; set; }
    }
}
