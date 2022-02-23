using System.ComponentModel.DataAnnotations;

namespace kekes.Models
{
    public class TagSubscribeViewModel
    {
        [Required]
        public string Text { get; set; }
    }
}
