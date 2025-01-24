using Bureau.UI.API.Features.Recipes.Models;
using System.ComponentModel.DataAnnotations;

namespace Bureau.UI.API.Features.Recipes.V1.Models
{
    public class RecipeRequestModel : BaseRecipeRequestModel
    {
        [Required]
        public required List<string> Ingredients { get; set; }
        public string? Instructions { get; set; }

    }
}
