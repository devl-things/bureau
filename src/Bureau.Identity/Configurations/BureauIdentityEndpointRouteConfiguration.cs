using Bureau.Identity.Managers;
using Bureau.Identity.Models;
using Bureau.Identity.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Bureau.Identity.Configurations
{
    public static class BureauIdentityEndpointRouteConfiguration
    {
        public static void MapBureauIdentityServerAPIEndpoints(this IEndpointRouteBuilder app) 
        {
            app.MapIdentityApi<ApplicationUser>();

            // provide an endpoint to clear the cookie for logout
            //
            // For more information on the logout endpoint and antiforgery, see:
            // https://learn.microsoft.com/aspnet/core/blazor/security/webassembly/standalone-with-identity#antiforgery-support
            app.MapPost("/logout", async (SignInManager<ApplicationUser> signInManager, [FromBody] object empty) =>
            {
                if (empty is not null)
                {
                    await signInManager.SignOutAsync();

                    return Results.Ok();
                }

                return Results.Unauthorized();
            }).RequireAuthorization();

            app.MapGet("/roles", (ClaimsPrincipal user) =>
            {
                if (user.Identity is not null && user.Identity.IsAuthenticated)
                {
                    var identity = (ClaimsIdentity)user.Identity;
                    var roles = identity.FindAll(identity.RoleClaimType)
                        .Select(c =>
                            new
                            {
                                c.Issuer,
                                c.OriginalIssuer,
                                c.Type,
                                c.Value,
                                c.ValueType
                            });

                    return TypedResults.Json(roles);
                }

                return Results.Unauthorized();
            }).RequireAuthorization();
        }


    }
}
