using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Sven.Services;
using Sven.Configurations;
using Sven.Models;
using Sven.Tests.Fixtures;
using Sven.Tests.TestData;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Sven.Tests.BureauFeatures
{
    [Trait("Category", "BureauFeatures")]
    public class BureauFeaturesRegistrationTests : IClassFixture<SvenWebAppFactory>, IDisposable
    {
        private readonly SvenWebAppFactory _factory;
        private readonly HttpClient _client;

        private const string TestIat = TestDataConstants.TestIat;
        private const string TestValidFeatureKey = "watson.nodes.crud";

        public BureauFeaturesRegistrationTests(SvenWebAppFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        private static StringContent BuildRegistrationRequest(Dictionary<string, List<string>>? bureauFeatures = null)
        {
            Dictionary<string, object> body = new Dictionary<string, object>
            {
                ["redirect_uris"] = new[] { "https://app.localhost/callback" },
                ["grant_types"] = new[] { AuthConstants.OAuth.GrantTypes.AuthorizationCode },
                ["response_types"] = new[] { AuthConstants.OAuth.ResponseTypes.Code },
                ["token_endpoint_auth_method"] = AuthConstants.OAuth.TokenAuthMethods.None,
                ["scope"] = AuthConstants.Scopes.OpenId,
            };
            if (bureauFeatures != null)
            {
                body["bureau_features"] = bureauFeatures;
            }
            string json = JsonSerializer.Serialize(body);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        [Fact]
        public async Task Register_WithValidBureauFeatures_Returns201WithFeaturesEchoed()
        {
            // VAULT-01 SC1: POST /oidc/register with valid bureau_features returns 201 with bureau_features echoed
            Dictionary<string, List<string>> bureauFeatures = new Dictionary<string, List<string>>
            {
                [TestValidFeatureKey] = new List<string>()
            };

            HttpResponseMessage response = await _client.PostAsync(
                Endpoints.Oidc.Register,
                BuildRegistrationRequest(bureauFeatures));

            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            using JsonDocument doc = JsonDocument.Parse(content);
            Assert.True(doc.RootElement.TryGetProperty("bureau_features", out JsonElement featuresElement),
                $"Response should contain bureau_features. Body: {content}");
            Assert.True(featuresElement.TryGetProperty(TestValidFeatureKey, out JsonElement _),
                $"bureau_features should contain key '{TestValidFeatureKey}'. Body: {content}");
        }

        [Fact]
        public async Task Register_WithUnknownFeatureKey_Returns400WithDescriptiveError()
        {
            // VAULT-01 SC2: POST /oidc/register with unknown feature key returns 400
            Dictionary<string, List<string>> bureauFeatures = new Dictionary<string, List<string>>
            {
                ["unknown.feature.xyz"] = new List<string>()
            };

            HttpResponseMessage response = await _client.PostAsync(
                Endpoints.Oidc.Register,
                BuildRegistrationRequest(bureauFeatures));

            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            using JsonDocument doc = JsonDocument.Parse(content);
            Assert.True(doc.RootElement.TryGetProperty("detail", out JsonElement detailElement),
                $"Response should contain detail. Body: {content}");
            string? detail = detailElement.GetString();
            Assert.NotNull(detail);
            Assert.Contains("unknown.feature.xyz", detail);
        }

        [Fact]
        public async Task GetClient_ReturnsBureauFeaturesAfterRegistration()
        {
            // VAULT-01 SC3: After successful registration, IClientService.GetClientAsync returns Client with BureauFeatures
            Dictionary<string, List<string>> bureauFeatures = new Dictionary<string, List<string>>
            {
                [TestValidFeatureKey] = new List<string>()
            };

            HttpResponseMessage response = await _client.PostAsync(
                Endpoints.Oidc.Register,
                BuildRegistrationRequest(bureauFeatures));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            string content = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(content);
            string? clientId = doc.RootElement.GetProperty("client_id").GetString();
            Assert.NotNull(clientId);

            using IServiceScope serviceScope = _factory.Services.CreateScope();
            IClientService clientService = serviceScope.ServiceProvider.GetRequiredService<IClientService>();
            Bureau.Result<Client> clientResult = await clientService.GetClientAsync(clientId, CancellationToken.None);

            Assert.False(clientResult.IsError,
                $"GetClientAsync should succeed. Error: {(clientResult.IsError ? clientResult.Error.ToString() : string.Empty)}");
            Assert.NotNull(clientResult.Value.BureauFeatures);
            Assert.True(clientResult.Value.BureauFeatures.ContainsKey(TestValidFeatureKey),
                $"BureauFeatures should contain key '{TestValidFeatureKey}'.");
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
