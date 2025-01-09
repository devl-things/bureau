using Bureau.UI.API.V1.Methods;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Asp.Versioning.Conventions;
using Asp.Versioning.Builder;
using Bureau.UI.API.Configurations;

namespace Bureau.UI.API.V1.Configurations
{
    public static partial class BureauAPIWebAppConfiguration
    {
        internal static void MapRecipes(this RouteGroupBuilder apiGroupBuilder, ApiVersionSet versionSet) 
        {
            RouteGroupBuilder recipesGroup = apiGroupBuilder.MapGroup(BureauAPIRouteNames.RecipesGroup);

            recipesGroup.MapDelete("{id}", RecipesMethods.DeleteRecipe)
                .WithName($"{BureauAPIRouteNames.DeleteRecipes}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version1)
                .MapToApiVersion(BureauAPIVersion.Version2)
                .MapToApiVersion(BureauAPIVersion.Version3);
        }
        internal static void MapRecipesV1(this RouteGroupBuilder apiGroupBuilder, ApiVersionSet versionSet)
        {
            
            RouteGroupBuilder recipesGroup = apiGroupBuilder.MapGroup(BureauAPIRouteNames.RecipesGroup);

            recipesGroup.MapGet("", RecipesMethods.GetRecipes)
                .WithName($"{BureauAPIRouteNames.GetRecipes}-{BureauAPIVersion.Version1}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version1);
            recipesGroup.MapGet("{id}", RecipesMethods.GetRecipeById)
                .WithName($"{BureauAPIRouteNames.GetRecipeById}-{BureauAPIVersion.Version1}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version1);

            recipesGroup.MapPost("", RecipesMethods.CreateRecipe)
                .WithName($"{BureauAPIRouteNames.CreateRecipes}-{BureauAPIVersion.Version1}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version1);

            recipesGroup.MapPut("{id}", RecipesMethods.UpdateRecipe)
                .WithName($"{BureauAPIRouteNames.UpdateRecipes}-{BureauAPIVersion.Version1}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version1);
        }

        internal static void MapNotesV1(this RouteGroupBuilder apiGroupBuilder, ApiVersionSet versionSet)
        {
            RouteGroupBuilder entryRouteBuilder = apiGroupBuilder.MapGroup(BureauAPIRouteNames.NotesGroup);

            entryRouteBuilder.MapPost("text", NotesMethods.CreateTextNote)
                .WithName(BureauAPIRouteNames.CreateTextNote)
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version1);

            entryRouteBuilder.MapGet($"{{id}}", NotesMethods.GetNoteById)
               .WithName(BureauAPIRouteNames.GetTextNoteById)
               .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version1);
        }
    }
}
