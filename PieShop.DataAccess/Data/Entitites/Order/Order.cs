using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PieShop.DataAccess.Data.Entitites.Order
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string ZipCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? State { get; set; }
        public string Country { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal OrderTotal { get; set; }
        public DateTimeOffset OrderPlaced { get; set; }
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        public class OrderEntity : IEntityTypeConfiguration<Order>
        {
            public void Configure(EntityTypeBuilder<Order> builder)
            {
                builder.HasKey(p => p.OrderId);
                builder.Property(p => p.OrderId).HasDefaultValueSql("NEWSEQUENTIALID()");
                builder.Property(p => p.FirstName).HasMaxLength(50);
                builder.Property(p => p.LastName).HasMaxLength(50);
                builder.Property(p => p.AddressLine1).HasMaxLength(100);
                builder.Property(p => p.AddressLine2).HasMaxLength(100);
                builder.Property(p => p.ZipCode).HasMaxLength(10);
                builder.Property(p => p.City).HasMaxLength(50);
                builder.Property(p => p.State).HasMaxLength(10);
                builder.Property(p => p.Country).HasMaxLength(50);
                builder.Property(p => p.PhoneNumber).HasMaxLength(25);
                builder.Property(p => p.Email).HasMaxLength(50);

                // TODO: test this!!!
                ////builder.HasMany(p => p.OrderDetails)
                ////    .WithOne(p => p.Order)
                ////    .HasForeignKey(p => p.OrderId);
            }
        }
    }
}
