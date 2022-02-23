using kekes.Data;
using kekes.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kekes.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public NotificationsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<Notification>> GetNewNotifications()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            var newNotifications = _context.Notifications.Include(x => x.User).Where(x => x.User == user && !x.IsShown).ToList();

            foreach (var notification in newNotifications)
            {
                notification.IsShown = true;
            }
            await _context.SaveChangesAsync();

            return newNotifications;
        }
    }
}
