using kekes.Data.Models;

namespace kekes.Models
{
    public class SectionDetailsViewModel
    {
        public Guid SectionId { get; set; }
        public string SectionName { get; set; }
        public string SectionDescription { get; set; }
        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
