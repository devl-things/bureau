using Bureau.Core;
using Bureau.Core.Models;
using Bureau.Recipes.Handlers;
using Bureau.Recipes.Models;

namespace Bureau.Recipes.Managers
{
    //TODO userId?? as updatedAt
    internal class RecipeManager : IRecipeManager
    {
        private readonly IRecipeCommandHandler _recipeCommandHandler;
        private readonly IRecipeQueryHandler _recipeQueryHandler;
        public RecipeManager(IRecipeCommandHandler recipeCommandHandler, IRecipeQueryHandler recipeQueryHandler)
        {
            _recipeCommandHandler = recipeCommandHandler;
            _recipeQueryHandler = recipeQueryHandler;
        }

        public async Task<Result<RecipeDto>> UpdateRecipeAsync(RecipeDto recipeModel, CancellationToken cancellationToken)
        {
            Result<IReference> result = await _recipeCommandHandler.UpdateRecipeAsync(recipeModel, cancellationToken);
            if (result.IsError)
            {
                return result.Error;
            }

            Result<RecipeDto> finalResult = await _recipeQueryHandler.GetRecipeAsync(result.Value.Id, cancellationToken);
            if (finalResult.IsError)
            {
                return finalResult.Error;
            }
            return finalResult.Value;
        }

        public async Task<Result<RecipeDto>> InsertRecipeAsync(RecipeDto recipeModel, CancellationToken cancellationToken)
        {
            Result<IReference> result = await _recipeCommandHandler.InsertRecipeAsync(recipeModel, cancellationToken);
            if (result.IsError)
            {
                return result.Error;
            }

            Result<RecipeDto> finalResult = await _recipeQueryHandler.GetRecipeAsync(result.Value.Id, cancellationToken);
            if (finalResult.IsError)
            {
                return finalResult.Error;
            }
            return finalResult.Value;
        }
    }
}
