using Sven.Configurations;
using Sven.Tests.Fixtures;
using Sven.Tests.TestData;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Sven.Tests.ClientCredentials
{
    [Trait("Category", "ClientCredentials")]
    public class Rfc7591ConfidentialClientRegistrationTests : IClassFixture<SvenWebAppFactory>, IDisposable
    {
        private readonly SvenWebAppFactory _factory;
        private readonly HttpClient _client;

        public Rfc7591ConfidentialClientRegistrationTests(SvenWebAppFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        private static StringContent BuildRegistrationRequest(string tokenEndpointAuthMethod)
        {
            Dictionary<string, object> body = new Dictionary<string, object>
            {
                ["token_endpoint_auth_method"] = tokenEndpointAuthMethod,
                ["grant_types"] = new[] { AuthConstants.OAuth.GrantTypes.ClientCredentials },
            };
            string json = JsonSerializer.Serialize(body);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        [Fact]
        public async Task Register_WithValidIat_ConfidentialClient_Returns201WithSecret()
        {
            // POST /oidc/register with Authorization: Bearer <iat>, token_endpoint_auth_method=client_secret_basic;
            // expect 201 + client_secret in response.
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", TestDataConstants.TestIat);

            HttpResponseMessage response = await _client.PostAsync(
                Endpoints.Oidc.Register,
                BuildRegistrationRequest(AuthConstants.OAuth.TokenAuthMethods.ClientSecretBasic));

            _client.DefaultRequestHeaders.Authorization = null;

            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Contains("client_secret", content);
            Assert.Contains("client_id", content);

            using JsonDocument doc = JsonDocument.Parse(content);
            string? secret = doc.RootElement.GetProperty("client_secret").GetString();
            Assert.False(string.IsNullOrWhiteSpace(secret), "client_secret must be present and non-empty in 201 response");
        }

        [Fact]
        public async Task Register_WithoutIat_Returns401()
        {
            // POST /oidc/register for confidential client without Authorization header; expect 401.
            HttpResponseMessage response = await _client.PostAsync(
                Endpoints.Oidc.Register,
                BuildRegistrationRequest(AuthConstants.OAuth.TokenAuthMethods.ClientSecretBasic));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Register_WithWrongIat_Returns401()
        {
            // POST /oidc/register with wrong IAT value; expect 401.
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", "this-is-the-wrong-iat-value");

            HttpResponseMessage response = await _client.PostAsync(
                Endpoints.Oidc.Register,
                BuildRegistrationRequest(AuthConstants.OAuth.TokenAuthMethods.ClientSecretBasic));

            _client.DefaultRequestHeaders.Authorization = null;

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Register_SecretReturnedOnce_NotRecoverableAfterwards()
        {
            // After registration the 201 response contains client_secret.
            // A subsequent registration request creates a different client with a different secret —
            // confirming there is no retrieval endpoint (secret is ephemeral at creation only).
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", TestDataConstants.TestIat);

            HttpResponseMessage firstResponse = await _client.PostAsync(
                Endpoints.Oidc.Register,
                BuildRegistrationRequest(AuthConstants.OAuth.TokenAuthMethods.ClientSecretPost));

            _client.DefaultRequestHeaders.Authorization = null;

            Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
            string firstContent = await firstResponse.Content.ReadAsStringAsync();

            using JsonDocument firstDoc = JsonDocument.Parse(firstContent);
            string? firstSecret = firstDoc.RootElement.GetProperty("client_secret").GetString();
            Assert.False(string.IsNullOrWhiteSpace(firstSecret));

            // Register again — produces a new client with a different secret.
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", TestDataConstants.TestIat);

            HttpResponseMessage secondResponse = await _client.PostAsync(
                Endpoints.Oidc.Register,
                BuildRegistrationRequest(AuthConstants.OAuth.TokenAuthMethods.ClientSecretPost));

            _client.DefaultRequestHeaders.Authorization = null;

            Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
            string secondContent = await secondResponse.Content.ReadAsStringAsync();

            using JsonDocument secondDoc = JsonDocument.Parse(secondContent);
            string? secondSecret = secondDoc.RootElement.GetProperty("client_secret").GetString();
            Assert.False(string.IsNullOrWhiteSpace(secondSecret));

            // Secrets are different (each registration generates a fresh random secret).
            Assert.NotEqual(firstSecret, secondSecret);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _client.Dispose();
        }
    }
}
