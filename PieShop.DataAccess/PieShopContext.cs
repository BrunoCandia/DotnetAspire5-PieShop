using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PieShop.DataAccess.Data.Entitites.Category;
using PieShop.DataAccess.Data.Entitites.Order;
using PieShop.DataAccess.Data.Entitites.Pie;
using PieShop.DataAccess.Data.Entitites.ShoppingCart;
using System.Reflection;

namespace PieShop.DataAccess
{
    public class PieShopContext : IdentityDbContext
    {
        public PieShopContext(DbContextOptions<PieShopContext> options) : base(options)
        {
        }

        public DbSet<Pie> Pie { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<ShoppingCartItem> ShoppingCartItem { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderDetail> OrderDetail { get; set; }
        public DbSet<Ingredient> Ingredient { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Get the IEntityTypeConfiguration related to the entities.
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // This needs to be added when working with Identity and we override OnModelCreating method. Otherwise we will get the error below when adding a migration.
            // Unable to create a 'DbContext' of type 'PieShopContext'. The exception 'The entity type 'IdentityUserLogin<string>' requires a primary key to be defined. If you intended to use a keyless entity type, call 'HasNoKey' in 'OnModelCreating'.
            base.OnModelCreating(builder);
        }
    }
}
