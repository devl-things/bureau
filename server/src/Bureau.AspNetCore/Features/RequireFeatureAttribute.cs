using Bureau.Primitives.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bureau.AspNetCore.Features;

/// <summary>
/// Guards an API endpoint with a two-layer feature check:
/// 1. Master switch (FeaturesOptions.Disabled in appsettings) → 404 if disabled globally.
/// 2. JWT scope claim → 403 if the caller's token does not carry the required scope.
///
/// Usage: [RequireFeature(FeatureKeys.Watson.Nodes.Crud)]
///
/// Registration: call services.AddBureauFeatures(configuration) in Program.cs to bind FeaturesOptions.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequireFeatureAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _featureKey;

    public RequireFeatureAttribute(string featureKey)
    {
        _featureKey = featureKey;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Layer 1: master switch
        var featuresOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<FeaturesOptions>>().Value;

        if (!featuresOptions.IsEnabled(_featureKey))
        {
            context.Result = new NotFoundResult();
            return;
        }

        // Layer 2: JWT scope claim
        bool hasScope = context.HttpContext.User.Claims.Any(c => c.Type == "scope" && c.Value.Equals(_featureKey, StringComparison.OrdinalIgnoreCase));

        if (!hasScope)
        {
            context.Result = new ForbidResult();
        }
    }
}
