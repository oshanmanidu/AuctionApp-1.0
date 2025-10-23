using Microsoft.AspNetCore.SignalR;

namespace AuctionApi.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendBidNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveBid", message);
        }

        public async Task SendAuctionNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveAuction", message);
        }
    }
}