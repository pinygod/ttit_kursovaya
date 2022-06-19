using System.ComponentModel.DataAnnotations;

namespace kekes.Models
{
    public class PostCreateViewModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Text { get; set; }
        public Guid SectionId { get; set; }
    }
}
