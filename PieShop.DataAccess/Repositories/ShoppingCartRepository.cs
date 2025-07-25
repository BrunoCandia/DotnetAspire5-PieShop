using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PieShop.DataAccess.Data.Entitites.ShoppingCart;
using PieModel = PieShop.Models.Pie;
using ShoppingCartItemModel = PieShop.Models.ShoppingCart;

namespace PieShop.DataAccess.Repositories
{
    public class ShoppingCartRepository : IShoppingCartRepository
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PieShopContext _pieShopContext;
        private readonly ILogger<ShoppingCartRepository> _logger;

        private string? ShoppingCartId { get; set; }

        public ShoppingCartRepository(PieShopContext pieShopContext, IHttpContextAccessor httpContextAccessor, ILogger<ShoppingCartRepository> logger)
        {
            _pieShopContext = pieShopContext;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;

            GetAndSetShoppingCartId();
        }

        private void GetAndSetShoppingCartId()
        {
            var session = _httpContextAccessor?.HttpContext?.Session;

            var shoppingCartId = session?.GetString("ShoppingCartId") ?? Guid.NewGuid().ToString();

            session?.SetString("ShoppingCartId", shoppingCartId);

            ShoppingCartId = shoppingCartId;
        }

        public async Task AddToCartAsync(PieModel.Pie pie)
        {
            ////var pieEntity = new Pie
            ////{
            ////    PieId = pie.PieId,
            ////    Name = pie.Name,
            ////    ShortDescription = pie.ShortDescription,
            ////    LongDescription = pie.LongDescription,
            ////    AllergyInformation = pie.AllergyInformation,
            ////    Price = pie.Price,
            ////    CategoryId = pie.CategoryId,
            ////    Category = existingCategory
            ////};

            var shoppingCartItem = await _pieShopContext.ShoppingCartItem
                    .FirstOrDefaultAsync(s => s.Pie.PieId == pie.PieId && s.ShoppingCartId == ShoppingCartId);

            if (shoppingCartItem is null)
            {
                shoppingCartItem = new ShoppingCartItem
                {
                    ShoppingCartId = ShoppingCartId,
                    PieId = pie.PieId,
                    ////Pie = pieEntity,
                    Amount = 1
                };

                await _pieShopContext.ShoppingCartItem.AddAsync(shoppingCartItem);
            }
            else
            {
                shoppingCartItem.Amount++;
            }

            await _pieShopContext.SaveChangesAsync();
        }

        public async Task<int> RemoveFromCartAsync(PieModel.Pie pie)
        {
            ////var pieEntity = new Pie
            ////{
            ////    PieId = pie.PieId,
            ////    Name = pie.Name,
            ////    ShortDescription = pie.ShortDescription,
            ////    LongDescription = pie.LongDescription,
            ////    AllergyInformation = pie.AllergyInformation,
            ////    Price = pie.Price,
            ////    CategoryId = pie.CategoryId,
            ////    Category = new Data.Entitites.Category.Category
            ////    {
            ////        CategoryId = pie.Category.CategoryId,
            ////        Name = pie.Category.Name,
            ////    }
            ////};

            var shoppingCartItem = await _pieShopContext.ShoppingCartItem
                    .FirstOrDefaultAsync(s => s.Pie.PieId == pie.PieId && s.ShoppingCartId == ShoppingCartId);

            var localAmount = 0;

            if (shoppingCartItem != null)
            {
                if (shoppingCartItem.Amount > 1)
                {
                    shoppingCartItem.Amount--;
                    localAmount = shoppingCartItem.Amount;
                }
                else
                {
                    _pieShopContext.ShoppingCartItem.Remove(shoppingCartItem);
                }
            }

            await _pieShopContext.SaveChangesAsync();

            return localAmount;
        }

        public async Task ClearCartAsync()
        {
            var cartItems = _pieShopContext.ShoppingCartItem
                    .Where(cart => cart.ShoppingCartId == ShoppingCartId);

            _pieShopContext.ShoppingCartItem.RemoveRange(cartItems);

            await _pieShopContext.SaveChangesAsync();
        }

        public async Task<List<ShoppingCartItemModel.ShoppingCartItem>> GetShoppingCartItemsAsync()
        {
            _logger.LogInformation("Getting shopping cart items for ShoppingCartId: {ShoppingCartId}", ShoppingCartId);

            return await _pieShopContext.ShoppingCartItem
                           .AsNoTracking()
                           .Where(c => c.ShoppingCartId == ShoppingCartId)
                           .Include(s => s.Pie)
                           .Select(sci => new ShoppingCartItemModel.ShoppingCartItem
                           {
                               ShoppingCartId = sci.ShoppingCartId,
                               Amount = sci.Amount,
                               ShoppingCartItemId = sci.ShoppingCartItemId,
                               PieId = sci.PieId,
                               Pie = new PieModel.Pie
                               {
                                   PieId = sci.Pie.PieId,
                                   Name = sci.Pie.Name,
                                   ShortDescription = sci.Pie.ShortDescription,
                                   LongDescription = sci.Pie.LongDescription,
                                   AllergyInformation = sci.Pie.AllergyInformation,
                                   Price = sci.Pie.Price,
                                   ImageUrl = sci.Pie.ImageUrl,
                                   ImageThumbnailUrl = sci.Pie.ImageThumbnailUrl,
                                   IsPieOfTheWeek = sci.Pie.IsPieOfTheWeek,
                                   IsInStock = sci.Pie.IsInStock
                               }
                           })
                           .ToListAsync();
        }

        public async Task<decimal> GetShoppingCartTotalAsync()
        {
            var total = await _pieShopContext.ShoppingCartItem
                    .AsNoTracking()
                    .Where(c => c.ShoppingCartId == ShoppingCartId)
                    .Select(c => c.Pie.Price * c.Amount)
                    .SumAsync();

            _logger.LogInformation("Total for ShoppingCartId {ShoppingCartId} is {Total}", ShoppingCartId, total);

            return total;
        }
    }
}
