using System.ComponentModel.DataAnnotations;

namespace Bureau.UI.API.Features.Recipes.Models
{
    public class BaseRecipeLayer
    {
        [Required]
        public required string Name { get; set; }
        public string? Instructions { get; set; }
    }
}
