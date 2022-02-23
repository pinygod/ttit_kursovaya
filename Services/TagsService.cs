using kekes.Data;
using kekes.Data.Models;
using Microsoft.AspNetCore.Identity;

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
            var tag = _context.Tags.FirstOrDefault(x => x.Text == text);
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

        public async Task<UserTags> GetUserTags(IdentityUser user)
        {
            var userTags = _context.UserTags.FirstOrDefault(x => x.User == user);
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
            var post = _context.Posts.FirstOrDefault(x => x.Id == postId);
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
                _notificationsService.SendNotificationToUsers(users, "Добавлен новый пост с тегом " + text);
            }
        }

        public async Task SubscribeUserOnTagAsync(string text, IdentityUser user)
        {
            var tag = (await GetTagAsync(text)).Item1;
            var userTags = await GetUserTags(user);

            userTags.Tags.Add(tag);
            await _context.SaveChangesAsync();
        }
    }
}
