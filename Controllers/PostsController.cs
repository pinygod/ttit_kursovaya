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

namespace kekes.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserPermissionsService _userPermissions;

        public PostsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IUserPermissionsService userPermissions)
        {
            _context = context;
            _userManager = userManager;
            _userPermissions = userPermissions;
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
                .Include(p => p.Section).Include(p => p.User).Include(x => x.Attachments).Include(p => p.Comments).ThenInclude(x => x.User)
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

            var post = await _context.Posts.FindAsync(id);
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

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,Text,Created,LastActivity,SectionId")] Post post)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!this._userPermissions.CanEditPost(post))
                    {
                        return this.Unauthorized();
                    }
                    _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["SectionId"] = new SelectList(_context.Sections, "Id", "Id", post.SectionId);
            return View(post);
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
                .Include(p => p.Section)
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
            var post = await _context.Posts.FindAsync(id);
            if (!this._userPermissions.CanEditPost(post))
            {
                return this.Unauthorized();
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(CommentCreateViewModel model)
        {
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

        private bool PostExists(Guid id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}
