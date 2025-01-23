using Microsoft.AspNetCore.Mvc.Rendering;
using PieShop.Models.Pie;

namespace PieShop.UI.Models
{
    public class PieAddViewModel
    {
        public IEnumerable<SelectListItem>? Categories { get; set; }
        public Pie Pie { get; set; }

        public PieAddViewModel()
        {
        }

        public PieAddViewModel(Pie pie, IEnumerable<SelectListItem>? categories)
        {
            Pie = pie;
            Categories = categories;
        }
    }
}