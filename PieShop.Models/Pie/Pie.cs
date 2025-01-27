namespace PieShop.Models.Pie
{
    public class Pie
    {
        public Guid PieId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? LongDescription { get; set; }
        public string? AllergyInformation { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }
        public bool IsPieOfTheWeek { get; set; }
        public bool IsInStock { get; set; }
        public Guid CategoryId { get; set; }
        public Category.Category? Category { get; set; }
        public List<Ingredient>? Ingredients { get; set; } = new List<Ingredient>();
        public byte[] RowVersion { get; set; } = new byte[8];
    }
}
