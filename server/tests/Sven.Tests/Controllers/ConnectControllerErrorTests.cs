using Microsoft.AspNetCore.Mvc.Testing;
using Sven.Configurations;
using Sven.Tests.Fixtures;
using Sven.Tests.TestData;
using System.Collections.Specialized;
using System.Net;
using System.Web;

namespace Sven.Tests.Controllers
{
    public class ConnectControllerErrorTests : IClassFixture<SvenWebAppFactory>, IDisposable
    {
        private readonly HttpClient _client;
        public ConnectControllerErrorTests(SvenWebAppFactory factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }
        [Fact(DisplayName = $"{Endpoints.Connect.Authorize} {AuthConstants.OAuth.ErrorDescriptions.InvalidRedirectUriFormat}")]
        [Trait("Category", "Unit")]
        [Trait("Type", "Expected error")]
        public async Task AuthorizeAsync_InvalidRedirectUri_ReturnsBadRequest()
        {
            Dictionary<string, string> query = new Dictionary<string, string>
            {
                [AuthConstants.OAuth.FieldNames.ClientId] = TestDataConstants.TestClientId,
                [AuthConstants.OAuth.FieldNames.RedirectUri] = "invalid-uri",
                [AuthConstants.OAuth.FieldNames.ResponseTypeField] = AuthConstants.OAuth.ResponseTypes.Code,
                [AuthConstants.OAuth.FieldNames.CodeChallenge] = "valid_challenge",
                [AuthConstants.OAuth.FieldNames.CodeChallengeMethod] = AuthConstants.OAuth.CodeChallengeMethods.Sha256
            };

            HttpResponseMessage response = await _client.GetAsync($"{Endpoints.Connect.Authorize}?{BuildQuery(query)}");
            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(AuthConstants.OAuth.Errors.InvalidRequest, content);
            Assert.Contains(AuthConstants.OAuth.ErrorDescriptions.InvalidRedirectUriFormat, content);
        }

        [Fact(DisplayName = $"{Endpoints.Connect.Authorize} {AuthConstants.OAuth.ErrorDescriptions.UnsupportedResponseType}")]
        [Trait("Category", "Unit")]
        [Trait("Type", "Expected error")]
        public async Task AuthorizeAsync_UnsupportedResponseType_RedirectsWithError()
        {
            Dictionary<string, string> query = new Dictionary<string, string>
            {
                [AuthConstants.OAuth.FieldNames.ClientId] = TestDataConstants.TestClientId,
                [AuthConstants.OAuth.FieldNames.RedirectUri] = TestDataConstants.TestClientRedirectUri,
                [AuthConstants.OAuth.FieldNames.ResponseTypeField] = "invalid",
                [AuthConstants.OAuth.FieldNames.CodeChallenge] = "valid_challenge",
                [AuthConstants.OAuth.FieldNames.CodeChallengeMethod] = AuthConstants.OAuth.CodeChallengeMethods.Sha256
            };

            HttpResponseMessage response = await _client.GetAsync($"{Endpoints.Connect.Authorize}?{BuildQuery(query)}");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            string? location = HttpUtility.UrlDecode(response.Headers.Location?.ToString());
            Assert.Contains(AuthConstants.OAuth.Errors.UnsupportedResponseType, location);
            Assert.Contains(AuthConstants.OAuth.ErrorDescriptions.UnsupportedResponseType, location);
        }

        [Fact(DisplayName = $"{Endpoints.Connect.AuthorizeContinue} {AuthConstants.OAuth.ErrorDescriptions.MissingAuthorizationState}")]
        [Trait("Category", "Unit")]
        [Trait("Type", "Expected error")]
        public async Task CompleteAuthorizeAsync_MissingPkceCookie_ReturnsBadRequest()
        {
            HttpResponseMessage response = await _client.GetAsync(Endpoints.Connect.AuthorizeContinue);
            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(AuthConstants.OAuth.Errors.InvalidRequest, content);
            Assert.Contains(AuthConstants.OAuth.ErrorDescriptions.MissingAuthorizationState, content);
        }

        private string? BuildQuery(Dictionary<string, string> parameters)
        {
            NameValueCollection query = HttpUtility.ParseQueryString(string.Empty);
            foreach (KeyValuePair<string, string> param in parameters)
            {
                query[param.Key] = param.Value;
            }
            return query.ToString();
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
