using Bureau.Core.Models;
using System.Diagnostics.CodeAnalysis;

namespace Bureau.Recipes.Models
{
    public class RecipeSubGroupDto : IReference
    {
        public string Id { get; set; } = string.Empty;
        public required string Name { get; set; }
        public required List<string> Ingredients { get; set; }
        public string? Instructions { get; set; }

        [SetsRequiredMembers]
        public RecipeSubGroupDto(string id)
        {
            Id = id;
            Name = string.Empty;
            Ingredients = new List<string>(0);
        }
    }
}
