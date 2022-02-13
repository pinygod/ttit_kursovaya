using Microsoft.AspNetCore.Identity;

namespace kekes.Data.Models
{
    public class Post
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActivity { get; set; }
        public ICollection<PostAttachment> Attachments { get; set; }
        public Guid SectionId { get; set; }
        public Section Section { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Tag> Tags { get; set; }
        public IdentityUser User { get; set; }

    }
}
