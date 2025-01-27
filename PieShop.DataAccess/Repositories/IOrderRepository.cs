using PieShop.Models.Order;

namespace PieShop.DataAccess.Repositories
{
    public interface IOrderRepository
    {
        Task CreateOrderAsync(Order order);
        Task<IEnumerable<Order>> GetAllOrdersWithDetailsAsync();
        Task<Order?> GetOrderDetailByOrderIdAsync(Guid orderId);
    }
}
