using System.ComponentModel.DataAnnotations;

namespace kekes.Models
{
    public class CommentCreateViewModel
    {
        [Required]
        public Guid PostId { get; set; }
        [Required]
        public string Text { get; set; }
    }
}
