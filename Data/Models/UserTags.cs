using Microsoft.AspNetCore.Identity;

namespace kekes.Data.Models
{
    public class UserTags
    {
        public Guid Id { get; set; }
        public IdentityUser User { get; set; }
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }
}
