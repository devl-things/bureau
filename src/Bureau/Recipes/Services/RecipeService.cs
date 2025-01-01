using Bureau.Core;
using Bureau.Core.Factories;
using Bureau.Core.Models;
using Bureau.Core.Models.Data;
using Bureau.Core.Repositories;
using Bureau.Recipes.Factories;
using Bureau.Recipes.Models;

namespace Bureau.Recipes.Services
{
    [Obsolete("This interface is obsolete. Use IRecipeManager instead.")]
    internal class RecipeService : IRecipeService
    {
        private IRepository _repository;
        public RecipeService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<RecipeAggregate>> GetRecipeAggregateAsync(IReference id, CancellationToken cancellationToken)
        {
            Result<AggregateModel> result = await InternalGetRecipeAggregateAsync(id, cancellationToken).ConfigureAwait(false);

            if (result.IsError)
            {
                return result.Error;
            }

            return RecipeAggregateFactory.Create(result.Value);
        }

        private async Task<Result<AggregateModel>> InternalGetRecipeAggregateAsync(IReference id, CancellationToken cancellationToken) 
        {
            IdSearchRequest idSearchRequest = new IdSearchRequest()
            {
                FilterReferenceId = id,
                FilterRequestType = EdgeRequestType.RootNode,
                SelectReferences = EdgeRequestType.Edge | EdgeRequestType.TargetNode,
                SelectRecordTypes = RecordRequestType.Edges | RecordRequestType.TermEntries | RecordRequestType.FlexRecords,
            };

           return await _repository.FetchRecordsAsync(idSearchRequest, cancellationToken).ConfigureAwait(false);
        }

        public async Task<Result<RecipeAggregate>> UpdateRecipeAggregateAsync(RecipeAggregate aggregate, CancellationToken cancellationToken)
        {
            TermSearchRequest termSearchRequest = new TermSearchRequest()
            {
                Terms = aggregate.TermEntries.Select(x => x.Label).ToHashSet(),
                RequestType = TermRequestType.Label
            };

            Result<Dictionary<string, TermEntry>> termsResult = await _repository.FetchTermRecordsAsync(termSearchRequest, cancellationToken);

            if (termsResult.IsError)
            {
                return termsResult.Error;
            }
            Result<AggregateModel> newRecipe = AggregateModelFactory.Create(aggregate, termsResult.Value);
            if (newRecipe.IsError)
            {
                return newRecipe.Error;
            }

            Result<IReference> result;
            if (BureauReferenceFactory.IsTempReference(aggregate.RecipeReference))
            {
                result = await _repository.InsertAggregateAsync(newRecipe.Value, cancellationToken);
            }
            else
            {
                Result<AggregateModel> existingRecipe = await InternalGetRecipeAggregateAsync(aggregate.RecipeReference, cancellationToken).ConfigureAwait(false);
                if (existingRecipe.IsError)
                {
                    return existingRecipe.Error;
                }

                Result<ExtendedAggregateModel> updatedRecipe = AggregateModelFactory.UpdateExisting(existingRecipe.Value, newRecipe.Value);
                if (existingRecipe.IsError)
                {
                    return existingRecipe.Error;
                }

                result = await _repository.UpdateAggregateAsync(updatedRecipe.Value, cancellationToken);
            }
            if (result.IsError)
            {
                return result.Error;
            }
            Result<AggregateModel> newlyRecipeResult = await InternalGetRecipeAggregateAsync(result.Value, cancellationToken).ConfigureAwait(false);
            if (newlyRecipeResult.IsError)
            {
                return newlyRecipeResult.Error;
            }
            return RecipeAggregateFactory.Create(newlyRecipeResult.Value);
        }
    }
}
