using Bureau.Core;
using Bureau.Core.Services;
using Bureau.UI.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bureau.UI.API.Filters
{
    public class PaginationValidationFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            // Safely retrieve query parameters
            int? page = context.HttpContext.Request.Query.TryGetValue("page", out var pageString)
                && int.TryParse(pageString, out var pageValue) ? pageValue : (int?)null;

            int? limit = context.HttpContext.Request.Query.TryGetValue("limit", out var limitString)
                && int.TryParse(limitString, out var limitValue) ? limitValue : (int?)null;

            IPaginationValidationService validator = context.HttpContext.RequestServices.GetRequiredService<IPaginationValidationService>();

            Result result = validator.Validate(page, limit);
            if (result.IsError) 
            {
                return result.Error.ToApiResponse();
            }

            // Proceed to the next filter or endpoint handler
            return await next(context);
        }
    }
}