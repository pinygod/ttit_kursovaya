#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using kekes.Data;
using kekes.Data.Models;
using kekes.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using kekes.Services;
using kekes.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace kekes.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserPermissionsService _userPermissions;
        private readonly ITagsService _tags;
        private readonly IHubContext<NotificationHub> _hubContext;

        public PostsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IUserPermissionsService userPermissions, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _userPermissions = userPermissions;
            _hubContext = hubContext;
        }

        // GET: Posts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Posts.Include(p => p.Section);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Posts/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Section).Include(p => p.Tags).Include(p => p.User).Include(x => x.Attachments).Include(p => p.Comments).ThenInclude(x => x.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Posts/Create
        [Authorize]
        public IActionResult Create(Guid sectionId)
        {
            if (sectionId != Guid.Empty)
            {
                var model = new PostCreateViewModel() { SectionId = sectionId };
                return View(model);
            }
            return NotFound();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var section = await _context.Sections
                .FirstOrDefaultAsync(m => m.Id == model.SectionId);
                if (section == null)
                {
                    return NotFound();
                }
                var post = new Post
                {
                    Id = Guid.NewGuid(),
                    SectionId = model.SectionId,
                    Name = model.Name,
                    Text = model.Text,
                    Created = DateTime.Now,
                    LastActivity = DateTime.Now,
                    User = await _userManager.GetUserAsync(HttpContext.User)
                };
                _context.Add(post);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("displayNotification", $"Добавлен пост \"{post.Name}\" в разделе \"{section.Name}\"");

                return RedirectToAction("Details", "Sections", new { id = model.SectionId });
            }
            return View(model);
        }

        // GET: Posts/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
            if (post == null)
            {
                return NotFound();
            }
            if (!this._userPermissions.CanEditPost(post))
            {
                return this.Unauthorized();
            }

            var model = new PostEditViewModel
            {
                Id = post.Id,
                Name = post.Name,
                Text = post.Text
            };

            return View(model);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PostEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var post = await _context.Posts.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == model.Id);
                if (post == null)
                {
                    return NotFound();
                }
                if (!this._userPermissions.CanEditPost(post))
                {
                    return this.Unauthorized();
                }

                post.Name = model.Name;
                post.Text = model.Text;

                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "Posts", new { id = model.Id });
            }
            return View(model);
        }

        // GET: Posts/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Section).Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }
            if (!this._userPermissions.CanEditPost(post))
            {
                return this.Unauthorized();
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var post = await _context.Posts.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
            if (!this._userPermissions.CanEditPost(post))
            {
                return this.Unauthorized();
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Sections", new { id = post.SectionId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(CommentCreateViewModel model)
        {
            if (!this.HttpContext.User.Identity.IsAuthenticated)
            {
                return this.Unauthorized();
            }

            var post = await _context.Posts
            .FirstOrDefaultAsync(m => m.Id == model.PostId);
            if (post == null)
            {
                return NotFound();
            }

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                Text = model.Text,
                Created = DateTime.Now,
                PostId = model.PostId,
                User = await _userManager.GetUserAsync(HttpContext.User)

            };
            post.LastActivity = DateTime.Now;
            _context.Add(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Posts", new { id = model.PostId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTag(TagAddViewModel model)
        {
            var post = await _context.Posts
            .FirstOrDefaultAsync(m => m.Id == model.PostId);
            if (post == null)
            {
                return NotFound();
            }

            if (!_userPermissions.CanEditPost(post))
            {
                return this.Unauthorized();
            }

            await _tags.AddTagToPostAsync(model.Text, post.Id);

            return RedirectToAction("Details", "Posts", new { id = model.PostId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task SubscribeOnTag(TagSubscribeViewModel model)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            await _tags.SubscribeUserOnTagAsync(model.Text, user);
        }

        [Authorize(Roles = ApplicationRoles.Administrators)]
        [HttpPost, ActionName("DeleteComment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(Guid id)
        {
            var comment = await _context.Comments.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Posts", new { id = comment.PostId });
        }

        private bool PostExists(Guid id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}
