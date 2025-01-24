using System.ComponentModel.DataAnnotations;

namespace Bureau.UI.API.Features.Recipes.V2.Models
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
        public required List<string> Ingredients { get; set; }
        public string? Instructions { get; set; }
    }
}
