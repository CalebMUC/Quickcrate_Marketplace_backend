//using Authentication_and_Authorization_Api.Models;
using Microsoft.IdentityModel.Tokens;
using Minimart_Api.DTOS;
using Minimart_Api.Models;
using Minimart_Api.Mappings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Minimart_Api.DTOS.Authorization;
using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.Products;
using Minimart_Api.DTOS.Address;
using Minimart_Api.DTOS.Cart;
using Minimart_Api.DTOS.Notification;
using Minimart_Api.Repositories;
using Newtonsoft.Json;

namespace Minimart_Api.Services
{
    public class MyService : IMyService
    {
        private readonly IRepository _repository;
        private readonly OrderMapper _orderMapper;

        public MyService(IRepository repository, OrderMapper orderMapper)
        {
            _repository = repository;
            _orderMapper = orderMapper;
        }

        // Modern Identity-based operations
        public async Task<UserInfo> GetRefreshToken(string userID)
        {
            return await _repository.GetRefreshToken(userID);
        }

        public async void SaveRefreshToken(string JsonData)
        {
            _repository.SaveRefreshToken(JsonData);
        }

        // Category operations following the same simple pattern
        public async Task<List<object>> GetCategoriesAsync()
        {
            return await _repository.GetCategoriesAsync();
        }

        public async Task<object> GetCategoryAsync(Guid categoryId)
        {
            return await _repository.GetCategoryAsync(categoryId);
        }

        public async Task<object> CreateCategoryAsync(string JsonData)
        {
            return await _repository.CreateCategoryAsync(JsonData);
        }
    }
}
