using Bureau.AspNetCore.Problems;
using Bureau.AspNetCore.Tracing;
using Bureau.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Bureau.AspNetCore.Middleware
{
    public sealed class ApiExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiExceptionHandlingMiddleware> _logger;
        private readonly JsonSerializerOptions _json;

        public ApiExceptionHandlingMiddleware(RequestDelegate next, ILogger<ApiExceptionHandlingMiddleware> logger, IOptions<JsonOptions> jsonOptions)
        {
            _next = next;
            _logger = logger;
            _json = jsonOptions.Value.JsonSerializerOptions;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException ex) when (context.RequestAborted.IsCancellationRequested)
            {
                // Request aborted by client. Usually not an application error.
                string traceId = TraceIdAccessor.GetTraceId(context);
                _logger.Info(ex, "Request canceled by client. TraceId={TraceId}", traceId);

                // It’s common to not write a response here because the client is already gone.
                // If headers already started, we cannot write anyway.
                if (!context.Response.HasStarted)
                {
                    context.Response.Clear();
                    context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest; // not official, but commonly used
                }
            }
            catch (Exception ex)
            {
                string traceId = TraceIdAccessor.GetTraceId(context);
                _logger.LogError(ex, "Unhandled exception. TraceId={TraceId}", traceId);

                if (context.Response.HasStarted)
                {
                    // Too late to write a ProblemDetails response.
                    throw;
                }

                ProblemDetails problemDetails = ProblemDetailsFactory.Create(context, traceId);

                context.Response.Clear();
                context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json";

                await JsonSerializer.SerializeAsync(context.Response.Body, problemDetails, _json, context.RequestAborted);
            }
        }
    }
}
