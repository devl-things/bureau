using Bureau.Core;
using Bureau.Recipes.Handlers;
using Bureau.Recipes.Managers;
using Bureau.Recipes.Models;
using Bureau.UI.API.Models;
using Bureau.UI.API.V1.Mappers;
using Bureau.UI.API.V1.Models.Recipes;
using Microsoft.AspNetCore.Http;
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
            ApiResponse<RecipeResponseModel> response = new ApiResponse<RecipeResponseModel>()
            {
                Status = ApiResponse.StatusSuccess,
                Data = recipeResult.Value.ToResponseModel()
            };
            response.Data.Instructions = httpContext.GetRequestedApiVersion()!.ToString();
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
            Result<RecipeDto> recipe = await handler.GetRecipeAsync(id, cancellationToken).ConfigureAwait(false);
            if (recipe.IsSuccess)
            {
                ApiResponse<RecipeResponseModel> response = new ApiResponse<RecipeResponseModel>()
                {
                    Status = ApiResponse.StatusSuccess,
                    Data = recipe.Value.ToResponseModel()
                };
                return Results.Ok(response);
            }
            return Results.BadRequest(recipe.Error.ToApiResponse());
        }

        public static async Task<IResult> GetRecipes(CancellationToken cancellationToken, [FromServices] IRecipeQueryHandler handler, [FromQuery] int? page, [FromQuery] int? limit)
        {
            PaginatedResult<List<RecipeDto>> recipes = await handler.GetRecipesAsync(page, limit, cancellationToken).ConfigureAwait(false);
            if (recipes.IsSuccess)
            {
                ApiResponse<List<RecipeResponseModel>> response = new ApiResponse<List<RecipeResponseModel>>()
                {
                    Status = ApiResponse.StatusSuccess,
                    Data = new List<RecipeResponseModel>(recipes.Value.Count),
                    Pagination = new PaginationData(recipes.Pagination)
                };
                recipes.Value.ForEach(x => response.Data.Add(x.ToResponseModel()));
                return Results.Ok(response);
            }
            return Results.BadRequest(recipes.Error.ToApiResponse());
        }
    }
}
