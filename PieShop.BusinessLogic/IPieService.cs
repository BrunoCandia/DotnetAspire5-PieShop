using PieShop.Models.Pie;
using PieShop.Models.Shared;

namespace PieShop.BusinessLogic
{
    public interface IPieService
    {
        Task<Pie?> GetPieByPieIdAsync(Guid pieId);
        Task<IEnumerable<Pie>> GetAllPiesAsync();
        Task<IEnumerable<Pie>> GetPiesOfTheWeekAsync();
        Task<IEnumerable<Pie>> SearchPiesAsync(string searchQuery, string? categoryId);
        Task<PaginatedResponse<IEnumerable<Pie>>> GetPiesPaginatedAsync(string orderBy, bool orderByDescending, int pageNumber, int pageSize);
        Task<int> AddPieAsync(Pie pie);
        Task<int> UpdatePieAsync(Pie pie);
        Task<int> DeletePieAsync(Guid pieId);
        Task<Pie?> GetFullPieByPieIdAsync(Guid pieId);
    }
}
