using Bureau.Core;
using Bureau.Recipes.Models;

namespace Bureau.Recipes.Handlers
{
    public interface IRecipeQueryHandler
    {
        Task<Result<RecipeDto>> GetRecipeAsync(string id, CancellationToken cancellationToken = default);
        Task<PaginatedResult<List<RecipeDto>>> GetRecipesAsync(int? page, int? limit, CancellationToken cancellationToken);
    }
}
