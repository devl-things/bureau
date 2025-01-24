using Bureau.Managers;
using Bureau.Providers;
using Bureau.Recipes.Models;
using Bureau.UI.API.Configurations;
using Bureau.UI.API.Features.Recipes.V1.Mappers;
using Bureau.UI.API.Features.Recipes.V1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using RecipesMethodsGeneral = Bureau.UI.API.Features.Recipes.RecipesMethods;

namespace Bureau.UI.API.Features.Recipes.V1
{
    internal static class RecipesMethods
    {
        public static Task<IResult> CreateRecipe(
            CancellationToken cancellationToken,
            HttpContext httpContext,
            [FromBody] RecipeRequestModel recipe,
            [FromServices] IDtoManager<RecipeDto> manager,
            [FromServices] LinkGenerator linkGenerator)
        {
            return RecipesMethodsGeneral.CreateRecipeAsync(
                cancellationToken,
                httpContext,
                recipe,
                manager,
                linkGenerator,
                $"{BureauAPIRouteNames.GetRecipeById}-{BureauAPIVersion.Version1}",
                x => x.ToDto(), // V1-specific mapping logic
                x => x.ToResponseModel() // V1-specific mapping logic
            );
        }

        public static Task<IResult> UpdateRecipe(
            string id,
            CancellationToken cancellationToken,
            [FromBody] RecipeRequestModel recipe,
            [FromServices] IDtoManager<RecipeDto> manager)
        {
            return RecipesMethodsGeneral.UpdateRecipeAsync(
                id,
                cancellationToken,
                recipe,
                manager,
                (model, recipeId) => model.ToDto(recipeId), // V1-specific mapping logic
                x => x.ToResponseModel()
            );
        }

        public static Task<IResult> GetRecipeById(
            string id,
            CancellationToken cancellationToken,
            [FromServices] IDtoProvider<RecipeDto> provider)
        {
            return RecipesMethodsGeneral.GetRecipeByIdAsync(
                id,
                cancellationToken,
                provider,
                x => x.ToResponseModel() // V1-specific mapping logic
            );
        }

        public static Task<IResult> GetRecipes(
            CancellationToken cancellationToken,
            [FromServices] IDtoProvider<RecipeDto> provider,
            [FromQuery] int? page,
            [FromQuery] int? limit)
        {
            return RecipesMethodsGeneral.GetRecipesAsync(
                cancellationToken,
                provider,
                page,
                limit,
                x => x.ToResponseModel() // V1-specific mapping logic
            );
        }
    }
}
