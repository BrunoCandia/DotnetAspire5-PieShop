using Microsoft.AspNetCore.Mvc.Rendering;
using PieShop.Models.Pie;

namespace PieShop.UI.Models
{
    public class PieSearchViewModel
    {
        public IEnumerable<Pie> Pies { get; set; }
        public IEnumerable<SelectListItem>? Categories { get; set; }
        public string? SearchQuery { get; set; }
        public string? SearchCategory { get; set; }

        public PieSearchViewModel(IEnumerable<Pie> pies, IEnumerable<SelectListItem>? categories, string? searchQuery, string? searchCategory)
        {
            Pies = pies;
            Categories = categories;
            SearchQuery = searchQuery;
            SearchCategory = searchCategory;
        }
    }
}
