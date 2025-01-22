using Bureau.Core;
using Bureau.Core.Models;
using Bureau.Core.Models.Data;

namespace Bureau.Recipes.Handlers
{
    internal interface IInternalRecipeQueryHandler
    {
        Task<Result<InsertAggregateModel>> InternalGetRecipeAggregateAsync(IReference id, CancellationToken cancellationToken);
    }
}
