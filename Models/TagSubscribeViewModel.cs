using System.ComponentModel.DataAnnotations;

namespace kekes.Models
{
    public class TagSubscribeViewModel
    {
        [Required]
        public Guid TagId { get; set; }
    }
}
