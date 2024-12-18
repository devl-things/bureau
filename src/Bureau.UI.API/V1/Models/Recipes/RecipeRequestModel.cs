using System.ComponentModel.DataAnnotations;

namespace Bureau.UI.API.V1.Models.Recipes
{
    public class RecipeRequestModel
    {
        [Required]
        public required string Name { get; set; }
        [Required]
        public required List<string> Ingredients { get; set; }
        public string? Instructions { get; set; }
        public string? PreparationTime { get; set; }
        public int? Servings { get; set; }

    }
}
