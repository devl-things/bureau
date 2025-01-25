using Asp.Versioning.Builder;
using Asp.Versioning.Conventions;
using Bureau.Managers;
using Bureau.Providers;
using Bureau.Recipes.Models;
using Bureau.UI.API.Configurations;
using Bureau.UI.API.Features.Recipes.V2.Mappers;
using Bureau.UI.API.Features.Recipes.V2.Models;
using Bureau.UI.API.Filters;
using Bureau.UI.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Bureau.UI.API.Features.Recipes.V2.Configurations
{
    public static partial class BureauRecipeAPIWebAppConfiguration
    {
        internal static void MapRecipesV2(this RouteGroupBuilder recipesGroup, ApiVersionSet versionSet)
        {
            recipesGroup.MapGet("", 
                (CancellationToken cancellationToken,
                [FromServices] IDtoProvider<string, RecipeDto> provider,
                [FromQuery] int? page,
                [FromQuery] int? limit) =>
                {
                    return RecipesMethods.GetRecipesAsync(
                        cancellationToken,
                        provider,
                        page,
                        limit,
                        x => x.ToResponseModel() // V2-specific mapping logic
                    );
                })
                .AddEndpointFilter<PaginationValidationFilter>()
                .WithName($"{BureauAPIRouteNames.GetRecipes}-{BureauAPIVersion.Version2}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version2);
            recipesGroup.MapGet("{id}", 
                (string id,
                CancellationToken cancellationToken,
                [FromServices] IDtoProvider<string, RecipeDto> provider) =>
                {
                    return RecipesMethods.GetRecipeByIdAsync(
                        id,
                        cancellationToken,
                        provider,
                        x => x.ToResponseModel() // V2-specific mapping logic
                    );
                })
                .WithName($"{BureauAPIRouteNames.GetRecipeById}-{BureauAPIVersion.Version2}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version2);

            recipesGroup.MapPost("", 
                (CancellationToken cancellationToken,
                [FromBody] RecipeRequestModel recipe,
                [FromServices] IDtoManager<string, RecipeDto> manager,
                [FromServices] BureauLinkGenerator linkGenerator) =>
                {
                    return RecipesMethods.CreateRecipeAsync(
                    cancellationToken,
                        recipe,
                        manager,
                        linkGenerator,
                        $"{BureauAPIRouteNames.GetRecipeById}-{BureauAPIVersion.Version2}",
                        x => x.ToDto(), // V2-specific mapping logic
                        x => x.ToResponseModel() // V2-specific mapping logic
                    );
                })
                .WithName($"{BureauAPIRouteNames.CreateRecipes}-{BureauAPIVersion.Version2}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version2);

            recipesGroup.MapPut("{id}", 
                (string id,
                CancellationToken cancellationToken,
                [FromBody] RecipeRequestModel recipe,
                [FromServices] IDtoManager<string, RecipeDto> manager) =>
                {
                    return RecipesMethods.UpdateRecipeAsync(
                        id,
                        cancellationToken,
                        recipe,
                        manager,
                        (model, recipeId) => model.ToDto(recipeId), // V2-specific mapping logic
                        x => x.ToResponseModel()
                    );
                })
                .WithName($"{BureauAPIRouteNames.UpdateRecipes}-{BureauAPIVersion.Version2}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version2);
        }
    }
}
