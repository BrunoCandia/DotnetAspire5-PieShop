using PieShop.Models.Category;

namespace PieShop.DataAccess.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<int> AddCategoryAsync(Category category);
        Task<Category?> GetCategoryByCategoryIdAsync(Guid categoryId);
        Task<int> UpdateCategoryAsync(Category category);
        Task<int> DeleteCategoryAsync(Guid categoryId);
    }
}
