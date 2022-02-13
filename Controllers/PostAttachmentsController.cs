using kekes.Data;
using kekes.Data.Models;
using kekes.Models;
using kekes.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace kekes.Controllers
{
    [Authorize]
    public class PostAttachmentsController : Controller
    {
        private static readonly HashSet<String> AllowedExtensions = new HashSet<String> { ".jpg", ".jpeg", ".png", ".gif" };

        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IUserPermissionsService userPermissions;
        private readonly IHostingEnvironment hostingEnvironment;

        public PostAttachmentsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IUserPermissionsService userPermissions, IHostingEnvironment hostingEnvironment)
        {
            this.context = context;
            this.userManager = userManager;
            this.userPermissions = userPermissions;
            this.hostingEnvironment = hostingEnvironment;
        }

        // GET: PostAttachments/Create
        public async Task<IActionResult> Create(Guid? postId)
        {
            if (postId == null)
            {
                return this.NotFound();
            }

            var post = await this.context.Posts.Include(x => x.User)
                .SingleOrDefaultAsync(m => m.Id == postId);
            if (post == null
                || !this.userPermissions.CanEditPost(post)
                )
            {
                return this.NotFound();
            }

            this.ViewBag.Post = post;
            return this.View(new PostAttachmentEditModel());
        }

        // POST: PostAttachments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid? postId, PostAttachmentEditModel model)
        {
            if (postId == null)
            {
                return this.NotFound();
            }

            var post = await this.context.Posts.Include(x => x.User)
                .SingleOrDefaultAsync(m => m.Id == postId);
            if (post == null
                || !this.userPermissions.CanEditPost(post)
                )
            {
                return this.NotFound();
            }

            var fileName = Path.GetFileName(ContentDispositionHeaderValue.Parse(model.File.ContentDisposition).FileName?.Trim('"'));
            var fileExt = Path.GetExtension(fileName);
            if (!PostAttachmentsController.AllowedExtensions.Contains(fileExt))
            {
                this.ModelState.AddModelError(nameof(model.File), "This file type is prohibited");
            }

            if (this.ModelState.IsValid)
            {
                var postAttachment = new PostAttachment
                {
                    PostId = post.Id,
                    Created = DateTime.UtcNow,
                };

                var attachmentPath = Path.Combine(this.hostingEnvironment.WebRootPath, "attachments", postAttachment.Id.ToString("N") + fileExt);
                postAttachment.Path = $"/attachments/{postAttachment.Id:N}{fileExt}";
                using (var fileStream = new FileStream(attachmentPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read))
                {
                    await model.File.CopyToAsync(fileStream);
                }

                this.context.Add(postAttachment);
                await this.context.SaveChangesAsync();
                return this.RedirectToAction("Details", "Posts", new { id = post.Id });
            }

            this.ViewBag.Post = post;
            return this.View(model);
        }

        // GET: PostAttachments/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var postAttachment = await this.context.PostAttachments
                .Include(p => p.Post).ThenInclude(x=>x.User)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (postAttachment == null
                || !this.userPermissions.CanEditPost(postAttachment.Post)
                )
            {
                return this.NotFound();
            }

            return this.View(postAttachment);
        }

        // POST: PostAttachments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var postAttachment = await this.context.PostAttachments
                .Include(p => p.Post).ThenInclude(x => x.User)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (postAttachment == null
                || !this.userPermissions.CanEditPost(postAttachment.Post)
                )
            {
                return this.NotFound();
            }

            var attachmentPath = Path.Combine(this.hostingEnvironment.WebRootPath, "attachments", postAttachment.Id.ToString("N") + Path.GetExtension(postAttachment.Path));
            System.IO.File.Delete(attachmentPath);
            this.context.PostAttachments.Remove(postAttachment);
            await this.context.SaveChangesAsync();
            return this.RedirectToAction("Details", "Posts", new { id = postAttachment.PostId });
        }
    }
}
