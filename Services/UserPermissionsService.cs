using kekes.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace kekes.Services
{
    public class UserPermissionsService : IUserPermissionsService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserManager<IdentityUser> userManager;

        public UserPermissionsService(IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager)
        {
            this.userManager = userManager;
            this.httpContextAccessor = httpContextAccessor;
        }

        private HttpContext HttpContext => this.httpContextAccessor.HttpContext;

        public Boolean CanEditPost(Post post)
        {
            if (!this.HttpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            if (this.HttpContext.User.IsInRole(ApplicationRoles.Administrators))
            {
                return true;
            }

            return this.userManager.GetUserId(this.httpContextAccessor.HttpContext.User) == post.User.Id;
        }

        public Boolean CanEditPostComment(Comment postComment)
        {
            if (!this.HttpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            if (this.HttpContext.User.IsInRole(ApplicationRoles.Administrators))
            {
                return true;
            }

            return false;
        }

        public Boolean CanAddSection()
        {
            if (this.HttpContext.User.IsInRole(ApplicationRoles.Administrators))
            {
                return true;
            }

            return false;
        }
    }
}
