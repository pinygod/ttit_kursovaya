using kekes.Data.Models;

namespace kekes.Models
{
    public class SectionsIndexViewModel
    {
        public IEnumerable<Section> PopularSections { get; set; }
        public IEnumerable<Section> RecentSections { get; set;}
    }
}
