using PieShop.DataAccess.Repositories;
using PieShop.Models.Order;

namespace PieShop.BusinessLogic
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task CreateOrderAsync(Order order)
        {
            await _orderRepository.CreateOrderAsync(order);
        }

        public async Task<IEnumerable<Order>> GetAllOrdersWithDetailsAsync()
        {
            return await _orderRepository.GetAllOrdersWithDetailsAsync();
        }

        public async Task<Order?> GetOrderDetailByOrderIdAsync(Guid orderId)
        {
            return await _orderRepository.GetOrderDetailByOrderIdAsync(orderId);
        }
    }
}
