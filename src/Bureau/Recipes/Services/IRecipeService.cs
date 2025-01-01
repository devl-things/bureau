using Bureau.Core;
using Bureau.Core.Models;
using Bureau.Recipes.Models;

namespace Bureau.Recipes.Services
{
    [Obsolete("This interface is obsolete. Use IRecipeManager instead.")]
    internal interface IRecipeService
    {
        Task<Result<RecipeAggregate>> GetRecipeAggregateAsync(IReference id, CancellationToken cancellationToken);
        Task<Result<RecipeAggregate>> UpdateRecipeAggregateAsync(RecipeAggregate recipeAggregate, CancellationToken cancellationToken);
    }
}
