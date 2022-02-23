using kekes.Data;
using kekes.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace kekes.Services
{
    public class TagsService : ITagsService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationsService _notificationsService;

        public TagsService(ApplicationDbContext context, INotificationsService notificationsService)
        {
            _context = context;
            _notificationsService = notificationsService;
        }

        public async Task<Tuple<Tag, bool>> GetTagAsync(string text)
        {
            var tag = await _context.Tags.FirstOrDefaultAsync(x => x.Text == text);
            if (tag == default)
            {
                tag = new Tag
                {
                    Text = text
                };
                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();

                return new Tuple<Tag, bool>(tag, false);
            }

            return new Tuple<Tag, bool>(tag, true);
        }

        public async Task<Tag> GetTagAsync(Guid tagId)
        {
            return await _context.Tags.FirstOrDefaultAsync(x => x.Id == tagId);
        }

        public async Task<UserTags> GetUserTags(IdentityUser user)
        {
            var userTags = await _context.UserTags.FirstOrDefaultAsync(x => x.User == user);
            if (userTags == default)
            {
                userTags = new UserTags { User = user };
                _context.UserTags.Add(userTags);
                await _context.SaveChangesAsync();
            }

            return userTags;
        }

        public async Task AddTagToPostAsync(string text, Guid postId)
        {
            var post = await _context.Posts.Include(x => x.Tags).Include(x => x.Section).FirstOrDefaultAsync(x => x.Id == postId);
            if (post == default)
            {
                return;
            }

            var tag = await GetTagAsync(text);
            post.Tags.Add(tag.Item1);
            await _context.SaveChangesAsync();

            if (tag.Item2)
            {
                var users = _context.UserTags.Where(x => x.Tags.Contains(tag.Item1)).Select(x => x.User).ToList();
                await _notificationsService.SendNotificationToUsers(users, "Добавлен новый пост в разделе " + post.Section.Name + " с тегом " + text);
            }
        }

        public async Task SubscribeUserOnTagAsync(Guid tagId, IdentityUser user)
        {
            var tag = await GetTagAsync(tagId);
            if (tag == default)
            {
                return;
            }

            var userTags = await GetUserTags(user);
            userTags.Tags.Add(tag);

            await _context.SaveChangesAsync();
        }
    }
}
