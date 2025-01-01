using Bureau.UI.API.V1.Methods;
using Bureau.UI.API.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Asp.Versioning.Conventions;
using Asp.Versioning.Builder;


namespace Bureau.UI.API.V1.Configurations
{
    public static class BureauAPIWebAppConfiguration
    {
        internal static void MapRecipesV1(this RouteGroupBuilder apiGroupBuilder, ApiVersionSet versionSet)
        {
            RouteGroupBuilder recipesGroup = apiGroupBuilder.MapGroup(BureauAPIRouteNames.RecipesGroup);

            recipesGroup.MapGet("", RecipesMethods.GetRecipes)
                .WithName(BureauAPIRouteNames.GetRecipes)
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(1);
            recipesGroup.MapGet("{id}", RecipesMethods.GetRecipeById)
                .WithName(BureauAPIRouteNames.GetRecipeById)
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(1);

            recipesGroup.MapPost("", RecipesMethods.CreateRecipe)
                .WithName(BureauAPIRouteNames.CreateRecipes)
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(1);

            recipesGroup.MapPut("{id}", RecipesMethods.UpdateRecipe)
                .WithName(BureauAPIRouteNames.UpdateRecipes)
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(1);

            recipesGroup.MapDelete("{id}", RecipesMethods.DeleteRecipe)
                .WithName(BureauAPIRouteNames.DeleteRecipes)
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(1);
        }

        internal static void MapNotesV1(this RouteGroupBuilder apiGroupBuilder, ApiVersionSet versionSet)
        {
            RouteGroupBuilder entryRouteBuilder = apiGroupBuilder.MapGroup(BureauAPIRouteNames.NotesGroup);

            entryRouteBuilder.MapPost("text", NotesMethods.CreateTextNote)
                .WithName(BureauAPIRouteNames.CreateTextNote)
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(1);

            entryRouteBuilder.MapGet($"{{id}}", NotesMethods.GetNoteById)
               .WithName(BureauAPIRouteNames.GetTextNoteById)
               .WithApiVersionSet(versionSet)
                .MapToApiVersion(1);
        }
    }
}
