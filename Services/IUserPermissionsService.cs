using kekes.Data.Models;

namespace kekes.Services
{
    public interface IUserPermissionsService
    {
        Boolean CanEditPost(Post post);

        Boolean CanEditPostComment(Comment postComment);
    }
}
