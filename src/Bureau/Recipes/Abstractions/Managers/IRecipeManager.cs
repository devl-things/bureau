using Bureau.Core;
using Bureau.Recipes.Models;

namespace Bureau.Recipes.Managers
{
    public interface IRecipeManager
    {
        Task<Result<RecipeDto>> UpdateRecipeAsync(RecipeDto recipeModel, CancellationToken cancellationToken);
        Task<Result<RecipeDto>> InsertRecipeAsync(RecipeDto recipeModel, CancellationToken cancellationToken);
        Task<Result> DeleteRecipeAsync(string id, CancellationToken cancellationToken);
    }
}
