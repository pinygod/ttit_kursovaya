using Microsoft.AspNetCore.Identity;

namespace kekes.Data.Models
{
    public class Comment
    {
        public Guid Id { get; set; }
        public IdentityUser User { get; set; }
        public string Text { get; set; }
        public DateTime Created { get; set; }
        public Guid PostId { get; set; }
        public Post Post { get; set; }
    }
}
