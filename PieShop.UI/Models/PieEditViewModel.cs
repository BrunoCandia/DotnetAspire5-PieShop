using Microsoft.AspNetCore.Mvc.Rendering;
using PieShop.Models.Pie;

namespace PieShop.UI.Models
{
    public class PieEditViewModel
    {
        public IEnumerable<SelectListItem>? Categories { get; set; }
        public Pie Pie { get; set; }

        public PieEditViewModel()
        {
        }

        public PieEditViewModel(Pie pie, IEnumerable<SelectListItem>? categories)
        {
            Pie = pie;
            Categories = categories;
        }
    }
}