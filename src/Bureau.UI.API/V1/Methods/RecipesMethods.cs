using Bureau.Core;
using Bureau.Recipes.Handlers;
using Bureau.Recipes.Managers;
using Bureau.Recipes.Models;
using Bureau.UI.API.Mappers;
using Bureau.UI.API.Models;
using Bureau.UI.API.V1.Mappers;
using Bureau.UI.API.V1.Models.Recipes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Threading;

namespace Bureau.UI.API.V1.Methods
{
    internal static class RecipesMethods
    {
        public static RecipeResponseModel DummyResponse = new RecipeResponseModel()
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Novi",
            Ingredients = new List<string>()
        };
        public static RecipeResponseModel DummyResponse2 = new RecipeResponseModel()
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Novi 2",
            Ingredients = new List<string>()
        };
        public static async Task<IResult> CreateRecipe(
            CancellationToken cancellationToken,
            HttpContext httpContext,
            [FromBody] RecipeRequestModel recipe,
            [FromServices] IRecipeManager manager,
            [FromServices] LinkGenerator linkGenerator)
        {
            Result<RecipeDto> recipeResult = await manager.InsertRecipeAsync(recipe.ToDto(), cancellationToken).ConfigureAwait(false);
            if(recipeResult.IsError)
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

        public static IResult DeleteRecipe(string id)
        {
            ApiResponse response = new ApiResponse()
            {
                Status = ApiResponse.StatusSuccess,
                Message = "jesam"
            };

            // Dummy data for demonstration; you’d retrieve this from a database
            return Results.Ok(response);
        }


        // Handler method for the GET request
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

        //TODO [first] add pagination
        public static async Task<IResult> GetRecipes(CancellationToken cancellationToken, [FromServices] IRecipeQueryHandler handler)
        {
            PaginatedResult<List<RecipeDto>> recipes = await handler.GetRecipesAsync(cancellationToken).ConfigureAwait(false);
            if (recipes.IsSuccess)
            {
                ApiResponse<List<RecipeResponseModel>> response = new ApiResponse<List<RecipeResponseModel>>()
                {
                    Status = ApiResponse.StatusSuccess,
                    Data = new List<RecipeResponseModel>(recipes.Value.Count),
                    Pagination = recipes.Pagination.ToPaginationData()
                };
                recipes.Value.ForEach(x => response.Data.Add(x.ToResponseModel()));
                return Results.Ok(response);
            }
            return Results.BadRequest(recipes.Error.ToApiResponse());
        }
    }
}
