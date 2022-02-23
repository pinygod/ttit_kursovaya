using System.ComponentModel.DataAnnotations;

namespace kekes.Data.Models
{
    public class Announcement
    {
        public Guid Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Text { get; set; }
        public DateTime Date { get; set; }
    }
}
