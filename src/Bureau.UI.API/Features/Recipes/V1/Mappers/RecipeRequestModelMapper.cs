using Bureau.Recipes.Models;
using Bureau.UI.API.Features.Recipes.V1.Models;

namespace Bureau.UI.API.Features.Recipes.V1.Mappers
{
    internal static class RecipeRequestModelMapper
    {
        internal static RecipeDto ToDto(this RecipeRequestModel recipe)
        {
            return new RecipeDto()
            {
                Name = recipe.Name,
                SubGroups = new List<RecipeSubGroupDto>()
                {
                    new RecipeSubGroupDto(string.Empty)
                    {
                        Name = recipe.Name,
                        Ingredients = recipe.Ingredients.Select(x => new RecipeIngredient(x)).ToList(),
                        Instructions = recipe.Instructions
                    }
                },
                PreparationTime = recipe.PreparationTime,
                Servings = recipe.Servings,
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
