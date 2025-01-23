using PieShop.Models.Order;

namespace PieShop.BusinessLogic
{
    public interface IOrderService
    {
        Task CreateOrderAsync(Order order);
        Task<IEnumerable<Order>> GetAllOrdersWithDetailsAsync();
        Task<Order?> GetOrderDetailByOrderIdAsync(Guid orderId);
    }
}
