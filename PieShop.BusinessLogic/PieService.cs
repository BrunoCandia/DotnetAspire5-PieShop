using PieShop.DataAccess.Repositories;
using PieShop.Models.Pie;
using PieShop.Models.Shared;

namespace PieShop.BusinessLogic
{
    public class PieService : IPieService
    {
        private readonly IPieRepository _pieRepository;

        public PieService(IPieRepository pieRepository)
        {
            _pieRepository = pieRepository;
        }

        public async Task<Pie?> GetPieByPieIdAsync(Guid pieId)
        {
            return await _pieRepository.GetPieByPieIdAsync(pieId);
        }

        public async Task<IEnumerable<Pie>> GetAllPiesAsync()
        {
            return await _pieRepository.GetAllPiesAsync();
        }

        public async Task<IEnumerable<Pie>> GetPiesOfTheWeekAsync()
        {
            return await _pieRepository.GetPiesOfTheWeekAsync();
        }

        public async Task<IEnumerable<Pie>> SearchPiesAsync(string searchQuery, string? categoryId)
        {
            return await _pieRepository.SearchPiesAsync(searchQuery, categoryId);
        }

        public async Task<PaginatedResponse<IEnumerable<Pie>>> GetPiesPaginatedAsync(string orderBy, bool orderByDescending, int pageNumber, int pageSize)
        {
            return await _pieRepository.GetPiesPaginatedAsync(orderBy, orderByDescending, pageNumber, pageSize);
        }

        public async Task<int> AddPieAsync(Pie pie)
        {
            return await _pieRepository.AddPieAsync(pie);
        }

        public async Task<int> UpdatePieAsync(Pie pie)
        {
            return await _pieRepository.UpdatePieAsync(pie);
        }

        public async Task<int> DeletePieAsync(Guid pieId)
        {
            return await _pieRepository.DeletePieAsync(pieId);
        }

        public async Task<Pie?> GetFullPieByPieIdAsync(Guid pieId)
        {
            return await _pieRepository.GetFullPieByPieIdAsync(pieId);
        }
    }
}
