using Minimart_Api.DTOS;
using Minimart_Api.DTOS.Authorization;
using Minimart_Api.Models;

namespace Minimart_Api.Repositories
{
    public interface IRepository
    {
        // Modern Identity-based user operations
        Task<UserInfo> GetRefreshToken(string userId);
        Task SaveRefreshToken(string JsonData);

        // Category operations following the same pattern
        Task<List<object>> GetCategoriesAsync();
        Task<object> GetCategoryAsync(Guid categoryId);
        Task<object> CreateCategoryAsync(string JsonData);
    }
}
