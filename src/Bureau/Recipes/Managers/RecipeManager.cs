using Bureau.Core;
using Bureau.Recipes.Models;
using Bureau.Recipes.Services;

namespace Bureau.Recipes.Managers
{
    internal class RecipeManager : IRecipeManager
    {
        private readonly IRecipeService _recipeService;
        public RecipeManager(IRecipeService recipeService)
        {
            _recipeService = recipeService;
        }
        public async Task<Result<RecipeModel>> GetRecipeAsync(string id, CancellationToken cancellationToken = default)
        {
            Result<RecipeAggregate> recipeAggregateResult = await _recipeService.GetRecipeAggregateAsync(id, cancellationToken).ConfigureAwait(false);

            if(recipeAggregateResult.IsError)
            {
                return recipeAggregateResult.Error;
            }

            RecipeModel result = CreateModel(recipeAggregateResult.Value);

            return result;
        }

        private RecipeModel CreateModel(RecipeAggregate aggregate) 
        {        
            return new RecipeModel()
            {
                Id = aggregate.Header.Id,
                Name = aggregate.Header.Title,
                Ingredients = aggregate.Ingredients.Select(i => i.Title).ToList(),
                Instructions = aggregate.Details.Data.Instructions,
                PreparationTime = aggregate.Details.Data.PreparationTime,
                Servings = aggregate.Details.Data.Servings,
                CreatedAt = aggregate.Header.CreatedAt,
                UpdatedAt = aggregate.Header.UpdatedAt,
            };
        }
    }
}
