using Bureau.Core;
using Bureau.Core.Models;
using Bureau.Handlers;
using Bureau.Managers;
using Bureau.Providers;
using Bureau.Recipes.Models;
using System;

namespace Bureau.Recipes.Services
{
    //TODO userId?? as updatedAt
    internal class RecipeManager : IDtoManager<RecipeDto>
    {
        private readonly TimeProvider _timeProvider;
        private readonly ICommandHandler<RecipeDto> _recipeCommandHandler;
        private readonly IDtoProvider<RecipeDto> _recipeProvider;
        public RecipeManager(TimeProvider timeProvider, ICommandHandler<RecipeDto> recipeCommandHandler, IDtoProvider<RecipeDto> recipeProvider)
        {
            _timeProvider = timeProvider;
            _recipeCommandHandler = recipeCommandHandler;
            _recipeProvider = recipeProvider;
        }

        public Task<Result> DeleteAsync(string id, CancellationToken cancellationToken)
        {
            return _recipeCommandHandler.DeleteAsync(id, cancellationToken);
        }

        public async Task<Result<RecipeDto>> InsertAsync(RecipeDto dto, CancellationToken cancellationToken)
        {
            dto.CreatedAt = _timeProvider.GetUtcNow().UtcDateTime;
            dto.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;

            Result<IReference> result = await _recipeCommandHandler.InsertAsync(dto, cancellationToken);
            if (result.IsError)
            {
                return result.Error;
            }

            Result<RecipeDto> finalResult = await _recipeProvider.GetByIdAsync(result.Value.Id, cancellationToken);
            if (finalResult.IsError)
            {
                return finalResult.Error;
            }
            return finalResult.Value;
        }

        public async Task<Result<RecipeDto>> UpdateAsync(RecipeDto dto, CancellationToken cancellationToken)
        {
            dto.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;

            Result<IReference> result = await _recipeCommandHandler.UpdateAsync(dto, cancellationToken);
            if (result.IsError)
            {
                return result.Error;
            }

            Result<RecipeDto> finalResult = await _recipeProvider.GetByIdAsync(result.Value.Id, cancellationToken);
            if (finalResult.IsError)
            {
                return finalResult.Error;
            }
            return finalResult.Value;
        }
    }
}
