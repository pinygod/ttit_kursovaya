using Microsoft.AspNetCore.Identity;

namespace kekes.Data.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        public IdentityUser User { get; set; }
        public string Text { get; set; }
    }
}
