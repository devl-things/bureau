using Bureau.Core;
using Bureau.Recipes.Handlers;
using Bureau.Recipes.Managers;
using Bureau.Recipes.Models;
using Bureau.UI.API.Models;
using Bureau.UI.API.V1.Mappers;
using Bureau.UI.API.V1.Models.Recipes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Bureau.UI.API.V1.Methods
{
    internal static class RecipesMethods
    {
        public static async Task<IResult> CreateRecipe(
            CancellationToken cancellationToken,
            HttpContext httpContext,
            [FromBody] RecipeRequestModel recipe,
            [FromServices] IRecipeManager manager,
            [FromServices] LinkGenerator linkGenerator)
        {
            Result<RecipeDto> recipeResult = await manager.InsertRecipeAsync(recipe.ToDto(), cancellationToken).ConfigureAwait(false);
            if (recipeResult.IsError)
            {
                return Results.BadRequest(recipeResult.Error.ToApiResponse());
            }
            // Generate the full URL for the newly created note
            var url = linkGenerator.GetUriByAddress(
                httpContext,
                BureauAPIRouteNames.GetRecipeById, // Name of the route we want to link to
                new RouteValueDictionary
                {
                    { "id", recipeResult.Value.Id }
                }
            );
            ApiResponse<RecipeResponseModel> response = recipeResult.ToApiResponse<RecipeDto, RecipeResponseModel>(x => x.ToResponseModel());
            // Return Created result with the generated URL
            return Results.Created(url, response);
        }

        public static async Task<IResult> UpdateRecipe(
            string id,
            CancellationToken cancellationToken,
            [FromBody] RecipeRequestModel recipe,
            [FromServices] IRecipeManager manager)
        {
            Result<RecipeDto> response = await manager.UpdateRecipeAsync(recipe.ToDto(id), cancellationToken).ConfigureAwait(false);
            if (response.IsError)
            {
                return Results.BadRequest(response.Error.ToApiResponse());
            }
            return Results.Ok(response.Value);
        }

        public static async Task<IResult> DeleteRecipe(string id, CancellationToken cancellationToken, [FromServices] IRecipeManager manager)
        {
            Result response = await manager.DeleteRecipeAsync(id, cancellationToken).ConfigureAwait(false);
            if (response.IsError)
            {
                return Results.BadRequest(response.Error.ToApiResponse());
            }
            return Results.Ok(response.ToApiResponse("Deletion succeeded."));
        }

        public static async Task<IResult> GetRecipeById(string id, CancellationToken cancellationToken, [FromServices] IRecipeQueryHandler handler)
        {
            Result<RecipeDto> result = await handler.GetRecipeAsync(id, cancellationToken).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                return Results.Ok(result.ToApiResponse<RecipeDto, RecipeResponseModel>(x => x.ToResponseModel()));
            }
            return Results.BadRequest(result.Error.ToApiResponse());
        }

        public static async Task<IResult> GetRecipes(CancellationToken cancellationToken, [FromServices] IRecipeQueryHandler handler, [FromQuery] int? page, [FromQuery] int? limit)
        {
            PaginatedResult<List<RecipeDto>> values = await handler.GetRecipesAsync(page, limit, cancellationToken).ConfigureAwait(false);
            if (values.IsSuccess)
            {
                return Results.Ok(values.ToPagedApiResponse<RecipeDto, RecipeResponseModel>(x => x.ToResponseModel()));
            }
            return Results.BadRequest(values.Error.ToApiResponse());
        }
    }
}
