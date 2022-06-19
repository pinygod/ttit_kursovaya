using Microsoft.AspNetCore.Identity;

namespace kekes.Services
{
    public interface INotificationsService
    {
        Task SendNotificationToUsers(ICollection<IdentityUser> users, string text);
    }
}