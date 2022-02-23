using kekes.Data;
using kekes.Data.Models;
using kekes.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace kekes.Services
{
    public class NotificationsService : INotificationsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationsService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task SendNotificationToUsers(ICollection<IdentityUser> users, string text)
        {
            foreach (var user in users)
            {
                var notification = new Notification
                {
                    User = user,
                    Text = text
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }

            await _hubContext.Clients.All.SendAsync("displayNotification");
        }
    }
}
