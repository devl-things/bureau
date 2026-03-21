using Microsoft.AspNetCore.Mvc.Testing;
using Sven.Configurations;
using Sven.Tests.Fixtures;
using Sven.Tests.TestData;
using System.Net;

namespace Sven.Tests.Controllers
{
    public class ConnectControllerRevocationTests : IClassFixture<SvenWebAppFactory>, IDisposable
    {
        private readonly HttpClient _client;

        public ConnectControllerRevocationTests(SvenWebAppFactory factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                HandleCookies = true,
                AllowAutoRedirect = false
            });
        }

        [Fact(DisplayName = $"{Endpoints.Connect.Revocation} valid token - 200 OK")]
        [Trait("Category", "Integration")]
        [Trait("Status", "WIP")]
        public async Task Revocation_ValidToken_ReturnsOk()
        {
            string token = "valid_token";
            Dictionary<string, string> request = new Dictionary<string, string>
            {
                { AuthConstants.OAuth.FieldNames.Token, token },
                { AuthConstants.OAuth.FieldNames.ClientId, TestDataConstants.TestClientId }
            };
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, Endpoints.Connect.Revocation)
            {
                Content = new FormUrlEncodedContent(request)
            };

            HttpResponseMessage response = await _client.SendAsync(message);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact(DisplayName = $"{Endpoints.Connect.Revocation} token not in store - 200 OK")]
        [Trait("Category", "Integration")]
        [Trait("Status", "WIP")]
        public async Task Revocation_NonexistentToken_ReturnsOk()
        {
            Dictionary<string, string> request = new Dictionary<string, string>
            {
                { AuthConstants.OAuth.FieldNames.Token, "nonexistent_token" },
                { AuthConstants.OAuth.FieldNames.ClientId, TestDataConstants.TestClientId }
            };

            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, Endpoints.Connect.Revocation)
            {
                Content = new FormUrlEncodedContent(request)
            };

            HttpResponseMessage response = await _client.SendAsync(message);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact(DisplayName = $"{Endpoints.Connect.Revocation} missing token - {AuthConstants.OAuth.Errors.InvalidRequest}")]
        [Trait("Category", "Integration")]
        public async Task Revocation_NoTokenParameter_ReturnsInvalidRequest()
        {
            Dictionary<string, string> request = new Dictionary<string, string>
            {
                { AuthConstants.OAuth.FieldNames.ClientId, TestDataConstants.TestClientId }
            };
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, Endpoints.Connect.Revocation)
            {
                Content = new FormUrlEncodedContent(request)
            };

            HttpResponseMessage response = await _client.SendAsync(message);
            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(AuthConstants.OAuth.Errors.InvalidRequest, content);
        }

        [Fact(DisplayName = $"{Endpoints.Connect.Revocation} with missing client_id - {AuthConstants.OAuth.Errors.InvalidClient}")]
        [Trait("Category", "Integration")]
        public async Task Revocation_MissingClientId_ReturnsInvalidClient()
        {
            Dictionary<string, string> request = new Dictionary<string, string>
            {
                { AuthConstants.OAuth.FieldNames.Token, "some_token" },
                { AuthConstants.OAuth.FieldNames.ClientId, "" }
            };

            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, Endpoints.Connect.Revocation)
            {
                Content = new FormUrlEncodedContent(request)
            };

            HttpResponseMessage response = await _client.SendAsync(message);
            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(AuthConstants.OAuth.Errors.InvalidClient, content);
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
