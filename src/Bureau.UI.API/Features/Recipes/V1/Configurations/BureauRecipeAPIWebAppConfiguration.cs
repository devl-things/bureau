﻿using Asp.Versioning.Builder;
using Asp.Versioning.Conventions;
using Bureau.Managers;
using Bureau.Providers;
using Bureau.Recipes.Models;
using Bureau.UI.API.Configurations;
using Bureau.UI.API.Features.Recipes.V1.Mappers;
using Bureau.UI.API.Features.Recipes.V1.Models;
using Bureau.UI.API.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Bureau.UI.API.Features.Recipes.V1.Configurations
{
    public static partial class BureauRecipeAPIWebAppConfiguration
    {
        internal static void MapRecipesV1(this RouteGroupBuilder recipesGroup, ApiVersionSet versionSet)
        {
            recipesGroup.MapGet("", 
                (CancellationToken cancellationToken,
                [FromServices] IDtoProvider<RecipeDto> provider,
                [FromQuery] int? page,
                [FromQuery] int? limit) =>
                {
                    return RecipesMethods.GetRecipesAsync(
                        cancellationToken,
                        provider,
                        page,
                        limit,
                        x => x.ToResponseModel() // V1-specific mapping logic
                    );
                })
                .AddEndpointFilter<PaginationValidationFilter>()
                .WithName($"{BureauAPIRouteNames.GetRecipes}-{BureauAPIVersion.Version1}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version1);
            recipesGroup.MapGet("{id}", 
                (string id,
                CancellationToken cancellationToken,
                [FromServices] IDtoProvider<RecipeDto> provider) =>
                {
                    return RecipesMethods.GetRecipeByIdAsync(
                        id,
                        cancellationToken,
                        provider,
                        x => x.ToResponseModel() // V1-specific mapping logic
                    );
                })
                .WithName($"{BureauAPIRouteNames.GetRecipeById}-{BureauAPIVersion.Version1}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version1);

            recipesGroup.MapPost("", 
                (CancellationToken cancellationToken,
                HttpContext httpContext,
                [FromBody] RecipeRequestModel recipe,
                [FromServices] IDtoManager<RecipeDto> manager,
                [FromServices] LinkGenerator linkGenerator) =>
                {
                    return RecipesMethods.CreateRecipeAsync(
                    cancellationToken,
                        httpContext,
                        recipe,
                        manager,
                        linkGenerator,
                        $"{BureauAPIRouteNames.GetRecipeById}-{BureauAPIVersion.Version2}",
                        x => x.ToDto(), // V1-specific mapping logic
                        x => x.ToResponseModel() // V1-specific mapping logic
                    );
                })
                .WithName($"{BureauAPIRouteNames.CreateRecipes}-{BureauAPIVersion.Version1}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version1);

            recipesGroup.MapPut("{id}", 
                (string id,
                CancellationToken cancellationToken,
                [FromBody] RecipeRequestModel recipe,
                [FromServices] IDtoManager<RecipeDto> manager) =>
                {
                    return RecipesMethods.UpdateRecipeAsync(
                        id,
                        cancellationToken,
                        recipe,
                        manager,
                        (model, recipeId) => model.ToDto(recipeId), // V1-specific mapping logic
                        x => x.ToResponseModel()
                    );
                })
                .WithName($"{BureauAPIRouteNames.UpdateRecipes}-{BureauAPIVersion.Version1}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version1);
        }
    }
}
