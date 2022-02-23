namespace kekes.Data.Models
{
    public class Tag
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
