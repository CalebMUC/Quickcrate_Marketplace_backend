using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.DTOS.Authorization
{
    /// <summary>
    /// Logout request to revoke refresh token
    /// </summary>
    public class LogoutRequest
    {
        /// <summary>
        /// Refresh token to be revoked (optional - if not provided, all tokens for user will be revoked)
        /// </summary>
        public string? RefreshToken { get; set; }
    }
}