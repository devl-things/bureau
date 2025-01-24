using Bureau.Recipes.Models;
using Bureau.UI.API.Features.Recipes.V2.Models;

namespace Bureau.UI.API.Features.Recipes.V2.Mappers
{
    internal static class RecipeResponseModelMapper
    {
        internal static RecipeResponseModel ToResponseModel(this RecipeDto recipe)
        {
            return new RecipeResponseModel()
            {
                Id = recipe.Id,
                Name = recipe.Name,
                Layers = recipe.SubGroups.Select(x => x.ToRecipeLayer()),
                PreparationTime = recipe.PreparationTime,
                Servings = recipe.Servings,
                CreatedAt = recipe.CreatedAt,
                UpdatedAt = recipe.UpdatedAt
            };
        }

        internal static RecipeLayer ToRecipeLayer(this RecipeSubGroupDto group)
        {
            return new RecipeLayer()
            {
                Name = group.Name,
                Ingredients = group.Ingredients.Select(x => x.Ingredient).ToList(),
                Instructions = group.Instructions
            };
        }
    }
}
