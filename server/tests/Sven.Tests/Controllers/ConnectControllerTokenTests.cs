using Microsoft.AspNetCore.Mvc.Testing;
using Sven.Configurations;
using Sven.Tests.Fixtures;
using Sven.Tests.TestData;
using System.Net;

namespace Sven.Tests.Controllers
{
    public class ConnectControllerTokenTests : IClassFixture<SvenWebAppFactory>, IDisposable
    {
        private readonly HttpClient _client;
        public ConnectControllerTokenTests(SvenWebAppFactory factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }
        [Fact(DisplayName = $"{Endpoints.Connect.Token} {AuthConstants.OAuth.ErrorDescriptions.UnsupportedGrantType}")]
        [Trait("Category", "Unit")]
        [Trait("Type", "Expected error")]
        public async Task TokenAsync_InvalidGrantType_ReturnsBadRequest()
        {
            Dictionary<string, string> formData = new Dictionary<string, string>
            {
                [AuthConstants.OAuth.FieldNames.GrantTypeField] = "invalid_grant",
                [AuthConstants.OAuth.FieldNames.ClientId] = TestDataConstants.TestClientId,
                [AuthConstants.OAuth.FieldNames.RedirectUri] = TestDataConstants.TestClientRedirectUri
            };

            HttpResponseMessage response = await _client.PostAsync(Endpoints.Connect.Token, new FormUrlEncodedContent(formData));
            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(AuthConstants.OAuth.Errors.UnsupportedGrantType, content);
            Assert.Contains(AuthConstants.OAuth.ErrorDescriptions.UnsupportedGrantType, content);
        }

        [Fact(DisplayName = $"{Endpoints.Connect.Token} {AuthConstants.OAuth.ErrorDescriptions.CodeOrCodeVerifierMissing}")]
        [Trait("Category", "Unit")]
        [Trait("Type", "Expected error")]
        public async Task TokenAsync_AuthorizationCodeFlow_MissingCodeVerifier_ReturnsBadRequest()
        {
            Dictionary<string, string> formData = new Dictionary<string, string>
            {
                [AuthConstants.OAuth.FieldNames.GrantTypeField] = AuthConstants.OAuth.GrantTypes.AuthorizationCode,
                [AuthConstants.OAuth.FieldNames.Code] = "test-code",
                [AuthConstants.OAuth.FieldNames.ClientId] = TestDataConstants.TestClientId,
                [AuthConstants.OAuth.FieldNames.RedirectUri] = TestDataConstants.TestClientRedirectUri
            };

            HttpResponseMessage response = await _client.PostAsync(Endpoints.Connect.Token, new FormUrlEncodedContent(formData));
            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(AuthConstants.OAuth.Errors.InvalidRequest, content);
            Assert.Contains(AuthConstants.OAuth.ErrorDescriptions.CodeOrCodeVerifierMissing, content);
        }
        [Fact(DisplayName = $"{Endpoints.Connect.Token} {AuthConstants.OAuth.ErrorDescriptions.RefreshTokenNotFound}")]
        [Trait("Category", "Unit")]
        [Trait("Type", "Expected error")]
        public async Task TokenAsync_InvalidRefreshToken_ReturnsInvalidGrant()
        {
            Dictionary<string, string> formData = new Dictionary<string, string>
            {
                [AuthConstants.OAuth.FieldNames.GrantTypeField] = AuthConstants.OAuth.GrantTypes.RefreshToken,
                [AuthConstants.OAuth.FieldNames.RefreshToken] = "invalid_token",
                [AuthConstants.OAuth.FieldNames.ClientId] = TestDataConstants.TestClientId,
                [AuthConstants.OAuth.FieldNames.RedirectUri] = TestDataConstants.TestClientRedirectUri,
            };

            HttpResponseMessage response = await _client.PostAsync(Endpoints.Connect.Token, new FormUrlEncodedContent(formData));
            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(AuthConstants.OAuth.Errors.InvalidGrant, content);
            Assert.Contains(AuthConstants.OAuth.ErrorDescriptions.RefreshTokenNotFound, content);
        }
        [Fact(DisplayName = $"{Endpoints.Connect.Token} missing redirect uri {AuthConstants.OAuth.Errors.InvalidRequest}")]
        [Trait("Category", "Unit")]
        [Trait("Type", "Expected error")]
        public async Task TokenAsync_MissingRedirectUri_ReturnsBadRequest()
        {
            Dictionary<string, string> formData = new Dictionary<string, string>
            {
                [AuthConstants.OAuth.FieldNames.GrantTypeField] = "invalid_grant",
                [AuthConstants.OAuth.FieldNames.ClientId] = TestDataConstants.TestClientId
            };

            HttpResponseMessage response = await _client.PostAsync(Endpoints.Connect.Token, new FormUrlEncodedContent(formData));
            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(AuthConstants.OAuth.Errors.InvalidRequest, content);
        }
        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
