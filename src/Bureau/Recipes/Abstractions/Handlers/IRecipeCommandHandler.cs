using Bureau.Core;
using Bureau.Core.Models;
using Bureau.Recipes.Models;

namespace Bureau.Recipes.Handlers
{
    public interface IRecipeCommandHandler
    {
        Task<Result<IReference>> UpdateRecipeAsync(RecipeDto recipeModel, CancellationToken cancellationToken);
        Task<Result<IReference>> InsertRecipeAsync(RecipeDto recipeModel, CancellationToken cancellationToken);
        Task<Result> DeleteRecipeAsync(string id, CancellationToken cancellationToken);
    }
}
