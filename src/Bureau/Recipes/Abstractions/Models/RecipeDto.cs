namespace Bureau.Recipes.Models
{
    public class RecipeDto
    {
        public string Id { get; set; } = string.Empty;
        public required string Name { get; set; }
        public required List<RecipeSubGroupDto> SubGroups { get; set; }
        public string? PreparationTime { get; set; }
        public int? Servings { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public static RecipeDto EmptyRecipe()
        {
            return new RecipeDto()
            {
                Id = string.Empty,
                Name = string.Empty,
                SubGroups = new List<RecipeSubGroupDto>(),
            };
        }
    }
}
