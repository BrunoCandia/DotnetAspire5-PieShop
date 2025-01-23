using PieShop.Models.Category;

namespace PieShop.UI.Models
{
    public class CategoryDetailViewModel
    {
        public Category Category { get; set; }

        public CategoryDetailViewModel(Category category)
        {
            Category = category;
        }
    }
}