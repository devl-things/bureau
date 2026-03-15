using Microsoft.AspNetCore.Mvc.Testing;
using Sven.Tests.Fixtures;
using System.Net;

namespace Sven.Tests.Security
{
    [Trait("Category", "Phase1")]
    public class RateLimitingTests : IClassFixture<SvenWebAppFactory>
    {
        private readonly SvenWebAppFactory _factory;

        private static readonly Dictionary<string, string?> LowRateLimitConfig = new()
        {
            ["RateLimiting:TokenPermitLimit"] = "2",
            ["RateLimiting:TokenWindowSeconds"] = "60",
            ["RateLimiting:AuthorizePermitLimit"] = "2",
            ["RateLimiting:AuthorizeWindowSeconds"] = "60",
            ["RateLimiting:SignInPermitLimit"] = "2",
            ["RateLimiting:SignInWindowSeconds"] = "60"
        };

        public RateLimitingTests(SvenWebAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task TokenEndpoint_ExceedsRateLimit_Returns429()
        {
            SvenWebAppFactory lowLimitFactory = _factory.WithExtraConfig(LowRateLimitConfig);

            HttpClient client = lowLimitFactory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                AllowAutoRedirect = false
            });

            HttpResponseMessage response1 = await client.PostAsync("/connect/token", new FormUrlEncodedContent([]));
            HttpResponseMessage response2 = await client.PostAsync("/connect/token", new FormUrlEncodedContent([]));
            HttpResponseMessage response3 = await client.PostAsync("/connect/token", new FormUrlEncodedContent([]));

            Assert.Equal(HttpStatusCode.TooManyRequests, response3.StatusCode);
            Assert.True(response3.Headers.Contains("Retry-After"), "Retry-After header must be present on 429 response");
        }

        [Fact]
        public async Task AuthorizeEndpoint_ExceedsRateLimit_Returns429()
        {
            SvenWebAppFactory lowLimitFactory = _factory.WithExtraConfig(LowRateLimitConfig);

            HttpClient client = lowLimitFactory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                AllowAutoRedirect = false
            });

            HttpResponseMessage response1 = await client.GetAsync("/connect/authorize?client_id=test&redirect_uri=https%3A%2F%2Fexample.com&response_type=code&scope=openid");
            HttpResponseMessage response2 = await client.GetAsync("/connect/authorize?client_id=test&redirect_uri=https%3A%2F%2Fexample.com&response_type=code&scope=openid");
            HttpResponseMessage response3 = await client.GetAsync("/connect/authorize?client_id=test&redirect_uri=https%3A%2F%2Fexample.com&response_type=code&scope=openid");

            Assert.Equal(HttpStatusCode.TooManyRequests, response3.StatusCode);
            Assert.True(response3.Headers.Contains("Retry-After"), "Retry-After header must be present on 429 response");
        }

        [Fact]
        public async Task SignInPage_ExceedsRateLimit_Returns429()
        {
            SvenWebAppFactory lowLimitFactory = _factory.WithExtraConfig(LowRateLimitConfig);

            HttpClient client = lowLimitFactory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                AllowAutoRedirect = false
            });

            HttpResponseMessage response1 = await client.PostAsync("/connect/signin", new FormUrlEncodedContent([]));
            HttpResponseMessage response2 = await client.PostAsync("/connect/signin", new FormUrlEncodedContent([]));
            HttpResponseMessage response3 = await client.PostAsync("/connect/signin", new FormUrlEncodedContent([]));

            Assert.Equal(HttpStatusCode.TooManyRequests, response3.StatusCode);
            Assert.True(response3.Headers.Contains("Retry-After"), "Retry-After header must be present on 429 response");
        }
    }
}
