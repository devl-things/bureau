using Bureau.Recipes.Models;

namespace Bureau.UI.API.V1.Models.Recipes
{
    internal static class RecipeResponseModelMapper
    {
        internal static RecipeResponseModel ToResponseModel(this RecipeModel recipe)
        { 
            return new RecipeResponseModel()
            {
                Id = recipe.Id,
                Name = recipe.Name,
                Ingredients = recipe.Ingredients,
                Instructions = recipe.Instructions,
                PreparationTime = recipe.PreparationTime,
                Servings = recipe.Servings,
                CreatedAt = recipe.CreatedAt,
                UpdatedAt = recipe.UpdatedAt
            };
        }
    }
}
