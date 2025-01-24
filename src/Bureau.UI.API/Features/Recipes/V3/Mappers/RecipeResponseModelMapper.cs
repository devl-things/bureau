using Bureau.Recipes.Models;
using Bureau.UI.API.Features.Recipes.V3.Models.Recipes;
using RecipeIngredient = Bureau.UI.API.Features.Recipes.V3.Models.Recipes.RecipeIngredient;

namespace Bureau.UI.API.Features.Recipes.V3.Mappers
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
                Ingredients = group.Ingredients
                    .Select(x => new RecipeIngredient()
                    {
                        Name = x.Ingredient,
                        Quantity = x.Quantity.Quantity ?? 0,
                        Unit = x.Quantity.Unit
                    }).ToList(),
                Instructions = group.Instructions
            };
        }
    }
}
