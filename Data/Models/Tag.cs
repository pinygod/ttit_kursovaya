namespace kekes.Data.Models
{
    public class Tag
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid PostId { get; set; }
        public Post Post { get; set; }
    }
}
