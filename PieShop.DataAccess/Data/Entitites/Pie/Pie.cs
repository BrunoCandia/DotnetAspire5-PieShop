using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PieShop.DataAccess.Data.Entitites.Pie
{
    public class Pie
    {
        public Guid PieId { get; set; }
        ////public required string Name { get; set; }
        public string Name { get; set; } = string.Empty; //// this is translated as: Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
        public string? ShortDescription { get; set; }
        public string? LongDescription { get; set; }
        public string? AllergyInformation { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }
        public bool IsPieOfTheWeek { get; set; }
        public bool IsInStock { get; set; }
        public Guid CategoryId { get; set; }
        ////public required Category.Category Category { get; set; }
        public Category.Category Category { get; set; } = default!;
        public List<Ingredient>? Ingredients { get; set; }
        public byte[] RowVersion { get; set; } = new byte[8];

        public class PieEntity : IEntityTypeConfiguration<Pie>
        {
            public void Configure(EntityTypeBuilder<Pie> builder)
            {
                builder.HasKey(p => p.PieId);
                builder.Property(p => p.PieId).HasDefaultValueSql("NEWSEQUENTIALID()");
                builder.Property(p => p.Name).HasMaxLength(100);
                builder.Property(p => p.ShortDescription).HasMaxLength(255);
                builder.Property(p => p.LongDescription).HasMaxLength(2000);
                builder.Property(p => p.AllergyInformation).HasMaxLength(255);
                builder.Property(p => p.Price).HasColumnType("decimal(18,2)");
                builder.Property(p => p.ImageUrl).HasMaxLength(255);
                builder.Property(p => p.ImageThumbnailUrl).HasMaxLength(255);
                builder.Property(p => p.RowVersion).IsRowVersion();
            }
        }
    }
}
