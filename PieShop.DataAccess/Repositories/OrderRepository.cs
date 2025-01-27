using Microsoft.EntityFrameworkCore;
using PieShop.DataAccess.Data.Entitites.Order;
using OrderModel = PieShop.Models.Order;

namespace PieShop.DataAccess.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly PieShopContext _pieShopContext;
        private readonly IShoppingCartRepository _shopCartRepository;

        public OrderRepository(PieShopContext pieShopContext, IShoppingCartRepository shopCartRepository)
        {
            _pieShopContext = pieShopContext;
            _shopCartRepository = shopCartRepository;
        }

        public async Task CreateOrderAsync(OrderModel.Order order)
        {
            // TODO: review https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/#avoiding-dbcontext-threading-issues
            var shoppingCartItems = await _shopCartRepository.GetShoppingCartItemsAsync();
            var shoppingCartTotal = await _shopCartRepository.GetShoppingCartTotalAsync();

            var orderEntity = new Order
            {
                FirstName = order.FirstName,
                LastName = order.LastName,
                AddressLine1 = order.AddressLine1,
                AddressLine2 = order.AddressLine2,
                ZipCode = order.ZipCode,
                City = order.City,
                State = order.State,
                Country = order.Country,
                PhoneNumber = order.PhoneNumber,
                Email = order.Email,
                OrderTotal = shoppingCartTotal,
                OrderPlaced = DateTimeOffset.UtcNow,
                OrderDetails = new List<OrderDetail>()
                ////OrderDetails = order.OrderDetails.Select(x => new OrderDetail
                ////{
                ////    Amount = x.Amount,
                ////    Price = x.Price,
                ////    OrderId = x.OrderId,
                ////    PieId = x.PieId,
                ////}).ToList()
            };

            // This works fine
            ////foreach (var item in shoppingCartItems)
            ////{
            ////    var orderDetail = new OrderDetail
            ////    {
            ////        Amount = item.Amount,
            ////        PieId = item.PieId,
            ////        Price = item.Pie.Price,
            ////    };

            ////    orderEntity.OrderDetails.Add(orderDetail);
            ////}

            ////The orderEntity needs to be assigned when adding a list or we will get the error below
            ////The MERGE statement conflicted with the FOREIGN KEY constraint "FK_OrderDetail_Order_OrderId". The conflict occurred in database "PieShop", table "dbo.Order", column 'OrderId'.
            var orderDetailList = new List<OrderDetail>();

            foreach (var item in shoppingCartItems)
            {
                var orderDetail = new OrderDetail
                {
                    Amount = item.Amount,
                    PieId = item.PieId,
                    Price = item.Pie.Price,
                    Order = orderEntity,
                };

                orderDetailList.Add(orderDetail);
            }

            await _pieShopContext.OrderDetail.AddRangeAsync(orderDetailList);

            await _pieShopContext.Order.AddAsync(orderEntity);

            await _pieShopContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<OrderModel.Order>> GetAllOrdersWithDetailsAsync()
        {
            return await _pieShopContext.Order
                .AsNoTracking()
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Pie)
                .OrderBy(o => o.OrderId)
                .Select(o => new OrderModel.Order
                {
                    OrderId = o.OrderId,
                    FirstName = o.FirstName,
                    LastName = o.LastName,
                    AddressLine1 = o.AddressLine1,
                    AddressLine2 = o.AddressLine2,
                    ZipCode = o.ZipCode,
                    City = o.City,
                    State = o.State,
                    Country = o.Country,
                    PhoneNumber = o.PhoneNumber,
                    Email = o.Email,
                    OrderTotal = o.OrderTotal,
                    OrderPlaced = o.OrderPlaced,
                    OrderDetails = o.OrderDetails.Select(od => new OrderModel.OrderDetail
                    {
                        OrderDetailId = od.OrderDetailId,
                        Amount = od.Amount,
                        Price = od.Price,
                        OrderId = o.OrderId,
                        PieId = od.PieId
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<OrderModel.Order?> GetOrderDetailByOrderIdAsync(Guid orderId)
        {
            return await _pieShopContext.Order
                .AsNoTracking()
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Pie)
                .Where(o => o.OrderId == orderId)
                .OrderBy(o => o.OrderId)
                .Select(o => new OrderModel.Order
                {
                    OrderId = o.OrderId,
                    FirstName = o.FirstName,
                    LastName = o.LastName,
                    AddressLine1 = o.AddressLine1,
                    AddressLine2 = o.AddressLine2,
                    ZipCode = o.ZipCode,
                    City = o.City,
                    State = o.State,
                    Country = o.Country,
                    PhoneNumber = o.PhoneNumber,
                    Email = o.Email,
                    OrderTotal = o.OrderTotal,
                    OrderPlaced = o.OrderPlaced,
                    OrderDetails = o.OrderDetails.Select(od => new OrderModel.OrderDetail
                    {
                        OrderDetailId = od.OrderDetailId,
                        Amount = od.Amount,
                        Price = od.Price,
                        OrderId = o.OrderId,
                        PieId = od.PieId
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }
    }
}
