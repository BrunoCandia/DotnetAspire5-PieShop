namespace PieShop.Models.Pie
{
    public class Ingredient
    {
        public Guid IngredientId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
    }
}
