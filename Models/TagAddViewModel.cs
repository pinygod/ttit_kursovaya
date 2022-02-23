using System.ComponentModel.DataAnnotations;

namespace kekes.Models
{
    public class TagAddViewModel
    {
        [Required]
        public Guid PostId { get; set; }
        [Required]
        public string Text { get; set; }
    }
}
