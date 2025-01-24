using Bureau.UI.API.Features.Recipes.Models;
using System.ComponentModel.DataAnnotations;

namespace Bureau.UI.API.Features.Recipes.V2.Models
{
    public class RecipeRequestModel : BaseRecipeRequestModel
    {
        [Required]
        public required IEnumerable<RecipeLayer> Layers { get; set; }

    }
    public class RecipeLayer : BaseRecipeLayer
    {
        [Required]
        public required List<string> Ingredients { get; set; }
    }
}
