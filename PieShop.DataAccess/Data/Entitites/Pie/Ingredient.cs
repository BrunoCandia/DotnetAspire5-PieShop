using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PieShop.DataAccess.Data.Entitites.Pie
{
    public class Ingredient
    {
        public Guid IngredientId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;

        public class IngredientEntity : IEntityTypeConfiguration<Ingredient>
        {
            public void Configure(EntityTypeBuilder<Ingredient> builder)
            {
                builder.HasKey(p => p.IngredientId);
                builder.Property(p => p.IngredientId).HasDefaultValueSql("NEWSEQUENTIALID()");
                builder.Property(p => p.Name).HasMaxLength(100);
                builder.Property(p => p.Amount).HasMaxLength(20);
            }
        }
    }
}
