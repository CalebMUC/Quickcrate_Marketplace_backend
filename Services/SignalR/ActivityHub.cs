using Microsoft.AspNetCore.SignalR;

namespace Minimart_Api.Services.SignalR
{
    public class ActivityHub : Hub
    {
        public ActivityHub() { }

        public async Task NotifyNewUser(string username) {

            await Clients.All.SendAsync("ReceiveNewUser", $"New User Registered:{username}");
        }
        public async Task NotifyNewMerchant(string merchantName)
        {

            await Clients.All.SendAsync("ReceiveNewMerchant", $"New User Registered:{merchantName}");
        }
        public async Task NotifyNewOrder(string OrderId)
        {

            await Clients.All.SendAsync("ReceiveNewOrder", $"New User Registered:{OrderId}");
        }
    }
}
