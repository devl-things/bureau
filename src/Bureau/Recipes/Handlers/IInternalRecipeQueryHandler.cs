using Bureau.Core.Models.Data;
using Bureau.Core.Models;
using Bureau.Core;

namespace Bureau.Recipes.Handlers
{
    internal interface IInternalRecipeQueryHandler
    {
        Task<Result<AggregateModel>> InternalGetRecipeAggregateAsync(IReference id, CancellationToken cancellationToken);
    }
}
