//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Minimart_Api.Data;
//using Minimart_Api.Models;

//namespace Minimart_Api.Services.Migration
//{
//    public interface IUserMigrationService
//    {
//        Task<string> GetEffectiveUserIdAsync(int? legacyUserId = null, string? identityUserId = null);
//        Task<ApplicationUser?> GetUserByLegacyIdAsync(int legacyUserId);
//        Task<ApplicationUser?> GetApplicationUserAsync(string identityUserId);
//        Task<bool> IsUserMigratedAsync(int legacyUserId);
//        Task<string> GetUserDisplayNameAsync(int? legacyUserId = null, string? identityUserId = null);
//    }

//    public class UserMigrationService : IUserMigrationService
//    {
//        private readonly MinimartDBContext _context;
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly ILogger<UserMigrationService> _logger;

//        public UserMigrationService(
//            MinimartDBContext context,
//            UserManager<ApplicationUser> userManager,
//            ILogger<UserMigrationService> logger)
//        {
//            _context = context;
//            _userManager = userManager;
//            _logger = logger;
//        }

//        public async Task<string> GetEffectiveUserIdAsync(int? legacyUserId = null, string? identityUserId = null)
//        {
//            // If Identity user ID is provided, use it
//            if (!string.IsNullOrEmpty(identityUserId))
//            {
//                return identityUserId;
//            }

//            // If legacy user ID is provided, check if user has been migrated
//            if (legacyUserId.HasValue)
//            {
//                var migratedUser = await GetUserByLegacyIdAsync(legacyUserId.Value);
//                if (migratedUser != null)
//                {
//                    return migratedUser.Id;
//                }

//                // User hasn't been migrated yet, return legacy ID as string
//                return legacyUserId.Value.ToString();
//            }

//            throw new ArgumentException("Either legacyUserId or identityUserId must be provided");
//        }

//        //public async Task<ApplicationUser?> GetUserByLegacyIdAsync(int legacyUserId)
//        //{
//        //    return await _context.Users
//        //        .FirstOrDefaultAsync(u => u.LegacyUserId == legacyUserId);
//        //}

//        public async Task<ApplicationUser?> GetApplicationUserAsync(string identityUserId)
//        {
//            return await _context.Users
//                .FirstOrDefaultAsync(u => u.Id == identityUserId);
//        }

//        public async Task<bool> IsUserMigratedAsync(int legacyUserId)
//        {
//            return await _context.Users
//                .AnyAsync(u => u.LegacyUserId == legacyUserId);
//        }

//        public async Task<string> GetUserDisplayNameAsync(int? legacyUserId = null, string? identityUserId = null)
//        {
//            // Try Identity user first
//            if (!string.IsNullOrEmpty(identityUserId))
//            {
//                var identityUser = await _userManager.FindByIdAsync(identityUserId);
//                if (identityUser != null)
//                {
//                    return identityUser.DisplayName ?? 
//                           identityUser.UserName ?? 
//                           identityUser.Email ?? 
//                           $"{identityUser.FirstName} {identityUser.LastName}".Trim() ??
//                           "Unknown User";
//                }
//            }

//            // Try legacy user by ID - check if user has been migrated to Identity
//            if (legacyUserId.HasValue)
//            {
//                // First check if user has been migrated
//                var migratedUser = await GetUserByLegacyIdAsync(legacyUserId.Value);
//                if (migratedUser != null)
//                {
//                    return migratedUser.DisplayName ?? 
//                           migratedUser.UserName ?? 
//                           migratedUser.Email ?? 
//                           $"{migratedUser.FirstName} {migratedUser.LastName}".Trim() ??
//                           "Unknown User";
//                }

//                // If not migrated, we can't get user info from legacy system since it's removed
//                // Log this situation and return a placeholder
//                _logger.LogWarning("Attempting to get display name for legacy user ID {LegacyUserId} but user not migrated to Identity system", legacyUserId.Value);
//                return $"Legacy User {legacyUserId.Value}";
//            }

//            return "Unknown User";
//        }

//        /// <summary>
//        /// Helper method to migrate a legacy user ID to the Identity system
//        /// This would typically be called during user login or when accessing user data
//        /// </summary>
//        public async Task<string?> MigrateLegacyUserAsync(int legacyUserId)
//        {
//            try
//            {
//                // Check if user is already migrated
//                var existingUser = await GetUserByLegacyIdAsync(legacyUserId);
//                if (existingUser != null)
//                {
//                    return existingUser.Id;
//                }

//                // Since we removed the legacy Users table, we can't migrate automatically
//                // This would need to be done through a different process or manual intervention
//                _logger.LogWarning("Legacy user {LegacyUserId} needs to be migrated to Identity system", legacyUserId);
//                return null;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error migrating legacy user {LegacyUserId}", legacyUserId);
//                return null;
//            }
//        }

//        /// <summary>
//        /// Get all migrated users (ApplicationUsers with LegacyUserId set)
//        /// </summary>
//        //public async Task<List<ApplicationUser>> GetAllMigratedUsersAsync()
//        //{
//        //    return await _context.Users
//        //        .Where(u => u.LegacyUserId.HasValue)
//        //        .ToListAsync();
//        //}

//        /// <summary>
//        /// Get ApplicationUser by email (useful for user lookup)
//        /// </summary>
//        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
//        {
//            return await _userManager.FindByEmailAsync(email);
//        }

//        /// <summary>
//        /// Get ApplicationUser by username
//        /// </summary>
//        public async Task<ApplicationUser?> GetUserByUsernameAsync(string username)
//        {
//            return await _userManager.FindByNameAsync(username);
//        }
//    }
//}