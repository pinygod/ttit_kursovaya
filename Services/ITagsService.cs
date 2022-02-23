using kekes.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace kekes.Services
{
    public interface ITagsService
    {
        Task AddTagToPostAsync(string text, Guid postId);
        Task<Tuple<Tag, bool>> GetTagAsync(string text);
        Task<UserTags> GetUserTags(IdentityUser user);
        Task SubscribeUserOnTagAsync(Guid tagId, IdentityUser user);
    }
}