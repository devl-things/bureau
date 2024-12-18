namespace Bureau.Recipes.Models
{
    public class RecipeModel
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required List<string> Ingredients { get; set; }
        public string? Instructions { get; set; }
        public string? PreparationTime { get; set; }
        public int? Servings { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
