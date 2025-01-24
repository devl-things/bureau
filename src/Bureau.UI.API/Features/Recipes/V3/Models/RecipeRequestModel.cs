using Bureau.UI.API.Features.Recipes.Models;
using System.ComponentModel.DataAnnotations;

namespace Bureau.UI.API.Features.Recipes.V3.Models
{
    public class RecipeRequestModel : BaseRecipeRequestModel
    {
        [Required]
        public required IEnumerable<RecipeLayer> Layers { get; set; }

    }
    public class RecipeLayer : BaseRecipeLayer
    {
        [Required]
        public required List<RecipeIngredient> Ingredients { get; set; }
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
