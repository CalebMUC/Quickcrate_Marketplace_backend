using System.Security.Claims;

namespace Minimart_Api.Services.CurrentUserServices
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly ClaimsPrincipal? _user;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _user = httpContextAccessor.HttpContext?.User;
        }

        public string UserId => _user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        public string UserName => _user?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

        public string Email => _user?.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

        public Guid MerchantId
        {
            get
            {
                var merchantIdClaim = _user?.FindFirst("merchant_id")?.Value;
                return !string.IsNullOrEmpty(merchantIdClaim) && Guid.TryParse(merchantIdClaim, out var merchantId)
                    ? merchantId
                    : Guid.Empty;
            }
        }

        public string Role => _user?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        public bool IsAdmin => Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

        public bool IsMerchant => Role.Equals("Merchant", StringComparison.OrdinalIgnoreCase);

        public bool IsStaff => Role.Equals("Staff", StringComparison.OrdinalIgnoreCase);

        public bool IsAuthenticated => _user?.Identity?.IsAuthenticated ?? false;

        public IEnumerable<string> Roles => _user?.FindAll(ClaimTypes.Role)?.Select(c => c.Value) ?? Enumerable.Empty<string>();

        public ClaimsPrincipal User => _user ?? new ClaimsPrincipal();

        public bool CanAccessMerchant(Guid merchantId)
        {
            // Admins can access any merchant
            if (IsAdmin) return true;

            // Merchants can only access their own data
            if (IsMerchant) return MerchantId == merchantId;

            // Staff cannot access merchant-specific data
            return false;
        }

        public bool IsInRole(string role) => _user?.IsInRole(role) ?? false;

        public string GetClaimValue(string claimType) => _user?.FindFirst(claimType)?.Value ?? string.Empty;

        public T GetClaimValue<T>(string claimType) where T : struct
        {
            var value = _user?.FindFirst(claimType)?.Value;
            if (string.IsNullOrEmpty(value))
                return default;

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default;
            }
        }
    }
}
