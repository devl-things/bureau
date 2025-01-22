using Bureau.Recipes.Models;
using Bureau.UI.API.V1.Models.Recipes;

namespace Bureau.UI.API.V1.Mappers
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
                CreatedAt = DateTime.Now.ToUniversalTime(),
                UpdatedAt = DateTime.Now.ToUniversalTime()
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
