using System.ComponentModel.DataAnnotations;

namespace Bureau.UI.API.Features.Recipes.Models
{
    public class BaseRecipeRequestModel
    {
        [Required]
        public required string Name { get; set; }
        public string? PreparationTime { get; set; }
        public int? Servings { get; set; }
    }
}
