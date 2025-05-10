using Microsoft.AspNetCore.Mvc.Testing;
using Sven.Configurations;
using Sven.PageModels;
using Sven.Tests.Fixtures;
using Sven.Tests.TestUtils;
using System.Net;

namespace Sven.Tests.Pages.Connect.SignIn
{
    public class PkceSignInTests : IClassFixture<SvenWebAppFactory>, IDisposable
    {
        private readonly HttpClient _client;
        public PkceSignInTests(SvenWebAppFactory factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact(DisplayName = $"{Endpoints.Connect.SignInPkce} GET {AuthConstants.OAuth.ErrorDescriptions.MissingAuthorizationState}")]
        [Trait("Category", "Unit")]
        [Trait("Type", "Expected error")]
        public async Task OnGet_MissingPkceCookie_ReturnsBadRequest()
        {
            HttpResponseMessage response = await _client.GetAsync(Endpoints.Connect.SignInPkce);
            string content = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(AuthConstants.OAuth.Errors.InvalidRequest, content);
            Assert.Contains(AuthConstants.OAuth.ErrorDescriptions.MissingAuthorizationState, content);
        }

        [Fact(DisplayName = $"{Endpoints.Connect.SignInPkce} GET {AuthConstants.OAuth.ErrorDescriptions.InvalidAuthorizationState}")]
        [Trait("Category", "Unit")]
        [Trait("Type", "Expected error")]
        public async Task OnGet_MissingPkceKeyInStore_ReturnsBadRequest()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Endpoints.Connect.SignInPkce);
            request.Headers.Add("Cookie", $"{AuthConstants.CookieNames.PkceKey}=your-value");

            HttpResponseMessage response = await _client.SendAsync(request);
            string content = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(AuthConstants.OAuth.Errors.InvalidRequest, content);
            Assert.Contains(AuthConstants.OAuth.ErrorDescriptions.InvalidAuthorizationState, content);
        }

        [Fact(DisplayName = $"{Endpoints.Connect.SignInPkce} POST {AuthConstants.OAuth.ErrorDescriptions.MissingAuthorizationState}")]
        [Trait("Category", "Unit")]
        [Trait("Type", "Expected error")]
        public async Task OnPostLoginAsync_MissingPkceCookie_ReturnsBadRequest()
        {
            HttpResponseMessage getPage = await _client.GetAsync(Endpoints.Connect.SignIn);
            string html = await getPage.Content.ReadAsStringAsync();

            string token = MiscHelper.GetAntiforgeryTokenFromContent(html);

            string url = $"{Endpoints.Connect.SignIn}?handler=Login";
            Dictionary<string, string> formData = new Dictionary<string, string>
            {
                [AuthConstants.OAuth.FieldNames.Username] = "test-user",
                [AuthConstants.OAuth.FieldNames.Password] = "test-pass",
                [AuthConstants.OAuth.FieldNames.Mode] = PageModelTypes.SignIn.Pkce,
                [MiscHelper.AntiforgeryFormKey] = token
            };

            HttpResponseMessage response = await _client.PostAsync(url, new FormUrlEncodedContent(formData));
            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(AuthConstants.OAuth.Errors.InvalidRequest, content);
            Assert.Contains(AuthConstants.OAuth.ErrorDescriptions.MissingAuthorizationState, content);
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
