using kekes.Data;
using kekes.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace kekes.Services
{
    public class NotificationsService : INotificationsService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public NotificationsService(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
        }
    }
}
