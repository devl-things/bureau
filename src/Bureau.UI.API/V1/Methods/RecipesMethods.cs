using Bureau.Core;
using Bureau.Recipes.Managers;
using Bureau.Recipes.Models;
using Bureau.UI.API.Models;
using Bureau.UI.API.V1.Models.Recipes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

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
        public static IResult CreateRecipe([FromBody] RecipeRequestModel recipe, [FromServices] LinkGenerator linkGenerator, HttpContext httpContext)
        {
            // Generate the full URL for the newly created note
            var url = linkGenerator.GetUriByAddress(
                httpContext,
                BureauAPIRouteNames.GetRecipeById, // Name of the route we want to link to
                new RouteValueDictionary
                {
                    { "id", DummyResponse.Id }
                }
            );
            ApiResponse<RecipeResponseModel> response = new ApiResponse<RecipeResponseModel>()
            {
                Status = ApiResponse.StatusSuccess,
                Data = DummyResponse
            };
            response.Data.Instructions = httpContext.GetRequestedApiVersion()!.ToString();
            // Return Created result with the generated URL
            return Results.Created(url, response);
        }

        public static IResult UpdateRecipe([FromBody] RecipeRequestModel recipe, [FromServices] LinkGenerator linkGenerator, HttpContext httpContext)
        {
            // Generate the full URL for the newly created note
            var url = linkGenerator.GetUriByAddress(
                httpContext,
                BureauAPIRouteNames.GetRecipeById, // Name of the route we want to link to
                new RouteValueDictionary
                {
                    { "id", DummyResponse.Id }
                }
            );
            ApiResponse<RecipeResponseModel> response = new ApiResponse<RecipeResponseModel>()
            {
                Status = ApiResponse.StatusSuccess,
                Data = DummyResponse
            };
            // Return Created result with the generated URL
            return Results.Created(url, response);
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
        public static async Task<IResult> GetRecipeById(string id, CancellationToken cancellationToken, [FromServices] IRecipeManager manager)
        {
            Result<RecipeModel> recipe = await manager.GetRecipeAsync(id, cancellationToken).ConfigureAwait(false);
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

        public static IResult GetRecipes()
        {
            ApiResponse<List<RecipeResponseModel>> response = new ApiResponse<List<RecipeResponseModel>>()
            {
                Status = ApiResponse.StatusSuccess,
                Data = new List<RecipeResponseModel>()
                {
                    DummyResponse,
                    DummyResponse2 
                }
            };
            // Dummy data for demonstration; you’d retrieve this from a database
            return Results.Ok(response);
        }
    }
}
