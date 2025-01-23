using PieShop.Models.Order;
using PieShop.Models.Pie;

namespace PieShop.UI.Models
{
    public class OrderIndexViewModel
    {
        public IEnumerable<Order>? Orders { get; set; }
        public IEnumerable<OrderDetail>? OrderDetails { get; set; }
        public IEnumerable<Pie>? Pies { get; set; }
        public Guid? SelectedOrderId { get; set; }
        public Guid? SelectedOrderDetailId { get; set; }
    }
}