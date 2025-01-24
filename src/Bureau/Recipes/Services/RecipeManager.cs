using Bureau.Core;
using Bureau.Core.Models;
using Bureau.Handlers;
using Bureau.Managers;
using Bureau.Providers;
using Bureau.Recipes.Models;

namespace Bureau.Recipes.Services
{
    //TODO userId?? as updatedAt
    internal class RecipeManager : IDtoManager<RecipeDto>
    {
        private readonly ICommandHandler<RecipeDto> _recipeCommandHandler;
        private readonly IDtoProvider<RecipeDto> _recipeProvider;
        public RecipeManager(ICommandHandler<RecipeDto> recipeCommandHandler, IDtoProvider<RecipeDto> recipeProvider)
        {
            _recipeCommandHandler = recipeCommandHandler;
            _recipeProvider = recipeProvider;
        }

        public Task<Result> DeleteAsync(string id, CancellationToken cancellationToken)
        {
            return _recipeCommandHandler.DeleteAsync(id, cancellationToken);
        }

        public async Task<Result<RecipeDto>> InsertAsync(RecipeDto dto, CancellationToken cancellationToken)
        {
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
