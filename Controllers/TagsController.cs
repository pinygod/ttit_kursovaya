using kekes.Data;
using kekes.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kekes.Controllers
{
    public class TagsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TagsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<Tag>> GetFollowingTags()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var tags = await _context.UserTags.Include(x => x.User).Include(x => x.Tags).FirstOrDefaultAsync(x => x.User == user);
            if (tags == default)
            {
                return new List<Tag>();
            }

            return tags.Tags;
        }
    }
}
