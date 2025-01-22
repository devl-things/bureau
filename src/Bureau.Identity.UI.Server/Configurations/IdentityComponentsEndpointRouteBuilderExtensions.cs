using Bureau.Identity.UI.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Bureau.Identity.Managers;
using Bureau.Identity.Providers;
using Bureau.Identity.Models;
using Bureau.Core;

namespace Bureau.Identity.UI.Server.Configurations
{
    public static class IdentityComponentsEndpointRouteBuilderExtensions
    {
        // These endpoints are required by the Identity Razor components defined in the /Components/Account/Pages directory of this project.


        public static IEndpointConventionBuilder MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
        {
            ArgumentNullException.ThrowIfNull(endpoints);

            var accountGroup = endpoints.MapGroup("/Account");

            accountGroup.MapPost("/PerformExternalLogin", (
                HttpContext context,
                [FromServices] ISignInManager signInManager,
                [FromForm] string provider,
                [FromForm] string returnUrl) =>
            {
                //TODO
                IEnumerable<KeyValuePair<string, StringValues>> query = [
                    new("ReturnUrl", returnUrl),
                    new("Action", BureauIdentityCallbackActionNames.Login)];

                var redirectUrl = UriHelper.BuildRelative(
                    context.Request.PathBase,
                    "/Account/ExternalLogin",
                    QueryString.Create(query));

                var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
                return TypedResults.Challenge(properties, [provider]);
            });

            accountGroup.MapPost("/Logout", async (
                ClaimsPrincipal user,
                ISignInManager signInManager,
                [FromForm] string returnUrl) =>
            {
                await signInManager.SignOutAsync();
                return TypedResults.LocalRedirect($"~/{returnUrl}");
            });

            var manageGroup = accountGroup.MapGroup("/Manage").RequireAuthorization();

            manageGroup.MapPost("/LinkExternalLogin", async (
                HttpContext context,
                [FromServices] ISignInManager signInManager,
                [FromServices] IIdentityProvider identityProvider,
                [FromForm] string provider) =>
            {
                // Clear the existing external cookie to ensure a clean login process
                await context.SignOutAsync(IdentityConstants.ExternalScheme);

                var redirectUrl = UriHelper.BuildRelative(
                    context.Request.PathBase,
                    "/Account/Manage/ExternalLogins",
                    QueryString.Create("Action", BureauIdentityCallbackActionNames.LinkLoginCallbackAction));
                Result<BureauUser> userResult = await identityProvider.GetUserAsync();
                var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, userResult.Value.Id);
                return TypedResults.Challenge(properties, [provider]);
            });

            var loggerFactory = endpoints.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var downloadLogger = loggerFactory.CreateLogger("DownloadPersonalData");

            manageGroup.MapPost("/DownloadPersonalData", async (
                HttpContext context,
                [FromServices] IIdentityProvider identityProvider,
                [FromServices] IUserManager userManager,
                [FromServices] IAccountManager accountManager
                //,
                //[FromServices] AuthenticationStateProvider authenticationStateProvider
                ) =>
            {
                Result<BureauUser> userResult = await identityProvider.GetUserAsync();
                if (userResult.IsError)
                {
                    return Results.NotFound($"Unable to load user with ID '{userResult.Value.Id}'.");
                }
                BureauUser user = userResult.Value;
                downloadLogger.LogInformation("User with ID '{UserId}' asked for their personal data.", user.Id);

                // Only include personal data for download
                var personalData = new Dictionary<string, string>();
                //TODO missing [PersonalData] in BureauUserModel
                var personalDataProps = typeof(BureauUser).GetProperties().Where(
                    prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
                foreach (var p in personalDataProps)
                {
                    personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
                }
                //ClaimsPrincipal claims = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;
                Result<List<BureauUserLoginModel>> loginsResult = await userManager.GetUserLoginsAsync(user);
                foreach (var l in loginsResult.Value)
                {
                    personalData.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
                }
                Result<string?> getAuthenticatorKeyResult = await accountManager.GetAuthenticatorKeyAsync(user);
                if (getAuthenticatorKeyResult.IsSuccess && string.IsNullOrWhiteSpace(getAuthenticatorKeyResult.Value))
                {
                    personalData.Add("Authenticator Key", getAuthenticatorKeyResult.Value!);
                }

                var fileBytes = JsonSerializer.SerializeToUtf8Bytes(personalData);

                context.Response.Headers.TryAdd("Content-Disposition", "attachment; filename=PersonalData.json");
                return TypedResults.File(fileBytes, contentType: "application/json", fileDownloadName: "PersonalData.json");
            });

            return accountGroup;
        }

    }
}
