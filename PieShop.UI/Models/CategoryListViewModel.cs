using PieShop.Models.Category;

namespace PieShop.UI.Models
{
    public class CategoryListViewModel
    {
        public IEnumerable<Category> Categories { get; set; } = [];
    }
}