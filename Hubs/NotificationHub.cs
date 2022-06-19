using Microsoft.AspNetCore.SignalR;

namespace kekes.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task Send(string message)
        {
            await this.Clients.All.SendAsync("Send", message);
        }
    }
}
