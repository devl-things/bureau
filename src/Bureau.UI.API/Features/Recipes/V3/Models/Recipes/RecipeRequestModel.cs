using System.ComponentModel.DataAnnotations;

namespace Bureau.UI.API.Features.Recipes.V3.Models.Recipes
{
    public class RecipeRequestModel
    {
        [Required]
        public required string Name { get; set; }
        [Required]
        public required IEnumerable<RecipeLayer> Layers { get; set; }
        public string? PreparationTime { get; set; }
        public int? Servings { get; set; }

    }
    public class RecipeLayer
    {
        [Required]
        public required string Name { get; set; }
        [Required]
        public required List<RecipeIngredient> Ingredients { get; set; }
        public string? Instructions { get; set; }
    }
    public class RecipeIngredient
    {
        [Required]
        public required string Name { get; set; }
        [Required]
        public required decimal Quantity { get; set; }
        public string? Unit { get; set; }
    }
}
