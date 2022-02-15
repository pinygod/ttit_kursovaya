using System.ComponentModel.DataAnnotations;

namespace kekes.Models
{
    public class SectionCreateViewModel
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
