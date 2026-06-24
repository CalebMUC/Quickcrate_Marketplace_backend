//using Authentication_and_Authorization_Api.Models;
using Minimart_Api.DTOS;
using Minimart_Api.DTOS.Address;
using Minimart_Api.DTOS.Authorization;
using Minimart_Api.DTOS.Cart;
using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.Notification;
using Minimart_Api.DTOS.Products;
using Minimart_Api.Models;
using System.Collections;

namespace Minimart_Api.Services
{
    public interface IMyService
    {
        

        

        //Task<ResponseStatus> CreateOrder(Order order);

        

        //Task<Order> GetOrderByIdAsync(string OrderID);

        
        
       





        Task <UserInfo> GetRefreshToken(string userID );

        

        //Task <LoginResponse> Login(string JsonData);

        public void SaveRefreshToken(string JsonData);

       // Task<UserRegStatus> UserRegistration(string JsonData);

        // Category methods following the same pattern
        Task<List<object>> GetCategoriesAsync();
        Task<object> GetCategoryAsync(Guid categoryId);
        Task<object> CreateCategoryAsync(string JsonData);
    }
}
