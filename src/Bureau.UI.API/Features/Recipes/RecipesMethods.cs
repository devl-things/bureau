using Bureau.Core;
using Bureau.Core.Models;
using Bureau.Managers;
using Bureau.Providers;
using Bureau.Recipes.Models;
using Bureau.UI.API.Models;
using Bureau.UI.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Bureau.UI.API.Features.Recipes
{
    internal static class RecipesMethods
    {
        public static async Task<IResult> DeleteRecipe(string id, CancellationToken cancellationToken, [FromServices] IDtoManager<string, RecipeDto> manager)
        {
            Result response = await manager.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            if (response.IsError)
            {
                return Results.BadRequest(response.Error.ToApiResponse());
            }
            return Results.Ok(response.ToApiResponse("Deletion succeeded."));
        }

        //TIPS httpContext.GetRequestedApiVersion()

        public static async Task<IResult> CreateRecipeAsync<TRequestModel, TResponseModel, TDto>(
                CancellationToken cancellationToken,
                TRequestModel recipe,
                IDtoManager<string, TDto> manager,
                BureauLinkGenerator linkGenerator,
                string routeName,
                Func<TRequestModel, TDto> toDto,
                Func<TDto, TResponseModel> toResponseModel)
                where TDto : IReference
        {
            Result<TDto> recipeResult = await manager.InsertAsync(toDto(recipe), cancellationToken).ConfigureAwait(false);
            if (recipeResult.IsError)
            {
                return Results.BadRequest(recipeResult.Error.ToApiResponse());
            }

            // Generate the URL for the created recipe
            var url = linkGenerator.GetLink(
                routeName,
                new RouteValueDictionary { { "id", recipeResult.Value.Id } }
            );

            ApiResponse<TResponseModel> response = recipeResult.ToApiResponse(toResponseModel);
            return Results.Created(url, response);
        }

        public static async Task<IResult> UpdateRecipeAsync<TRequestModel, TDto, TResponseModel>(
            string id,
            CancellationToken cancellationToken,
            TRequestModel recipe,
            IDtoManager<string, TDto> manager,
            Func<TRequestModel, string, TDto> toDto,
            Func<TDto, TResponseModel> toResponseModel)
            where TDto : class
        {
            Result<TDto> result = await manager.UpdateAsync(toDto(recipe, id), cancellationToken).ConfigureAwait(false);
            return PrepareResult(result, toResponseModel);
        }

        public static async Task<IResult> GetRecipeByIdAsync<TResponseModel, TDto>(
            string id,
            CancellationToken cancellationToken,
            IDtoProvider<string, TDto> provider,
            Func<TDto, TResponseModel> toResponseModel)
            where TDto : class
        {
            Result<TDto> result = await provider.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            return PrepareResult(result, toResponseModel);
        }

        public static async Task<IResult> GetRecipesAsync<TResponseModel, TDto>(
            CancellationToken cancellationToken,
            IDtoProvider<string, TDto> provider,
            int? page,
            int? limit,
            Func<TDto, TResponseModel> toResponseModel)
            where TDto : class
        {
            PaginatedResult<List<TDto>> values = await provider.GetAsync(string.Empty, page, limit, cancellationToken).ConfigureAwait(false);
            if (values.IsSuccess)
            {
                return Results.Ok(values.ToPagedApiResponse(x => toResponseModel(x)));
            }
            return Results.BadRequest(values.Error.ToApiResponse());
        }

        private static IResult PrepareResult<TDto, TResponseModel>(Result<TDto> result, Func<TDto, TResponseModel> toResponseModel)
        {
            if (result.IsSuccess)
            {
                return Results.Ok(result.ToApiResponse(x => toResponseModel(x)));
            }
            return Results.BadRequest(result.Error.ToApiResponse());
        }
    }
}
