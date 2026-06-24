using System.Security.Claims;

namespace Minimart_Api.Services.CurrentUserServices
{
    public interface ICurrentUserService
    {
        string UserId { get; }
        Guid MerchantId { get; }
        string Email { get; }
        string UserName { get; }
        bool IsAuthenticated { get; }
        IEnumerable<string> Roles { get; }
        ClaimsPrincipal User { get; }

        // New properties for role-based access
        string Role { get; }
        bool IsAdmin { get; }
        bool IsMerchant { get; }
        bool IsStaff { get; }
        bool CanAccessMerchant(Guid merchantId);

        bool IsInRole(string role);
        string GetClaimValue(string claimType);
        T GetClaimValue<T>(string claimType) where T : struct;
    }
}
