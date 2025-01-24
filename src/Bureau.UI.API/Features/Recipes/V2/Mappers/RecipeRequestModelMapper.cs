using Bureau.Recipes.Models;
using Bureau.UI.API.Features.Recipes.V2.Models;

namespace Bureau.UI.API.Features.Recipes.V2.Mappers
{
    internal static class RecipeRequestModelMapper
    {
        internal static RecipeDto ToDto(this RecipeRequestModel recipe)
        {
            RecipeDto result = new RecipeDto()
            {
                Name = recipe.Name,
                SubGroups = recipe.Layers.Select(x => x.ToSubGroupDto()).ToList(),
                PreparationTime = recipe.PreparationTime,
                Servings = recipe.Servings,
                CreatedAt = DateTime.Now.ToUniversalTime(),
                UpdatedAt = DateTime.Now.ToUniversalTime()
            };
            return result;
        }

        internal static RecipeSubGroupDto ToSubGroupDto(this RecipeLayer layer)
        {
            return new RecipeSubGroupDto(string.Empty)
            {
                Name = layer.Name,
                Ingredients = layer.Ingredients.Select(x => new RecipeIngredient(x)).ToList(),
                Instructions = layer.Instructions
            };
        }

        internal static RecipeDto ToDto(this RecipeRequestModel recipe, string id)
        {
            RecipeDto result = recipe.ToDto();
            result.Id = id;
            return result;
        }
    }
}
