using Bureau.Core;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Sven.Configurations;
using Sven.Models;
using Sven.PageModels.SignUp;
using Sven.Services;
using Sven.Tests.Fixtures;
using Sven.Tests.TestData;
using Sven.Tests.TestUtils;
using System.Net;

namespace Sven.Tests.Pages.Connect.SignUp
{
    public class PlainSignUpTests : IClassFixture<SvenWebAppFactory>, IDisposable
    {
        private readonly SvenWebAppFactory _factory;
        private readonly IStore<string, UserVerificationCode> _userVerificationStore;
        private readonly HttpClient _client;
        private readonly string _newUserEmail = TestDataConstants.NewUserEmail;
        private readonly string _newUserPass = TestDataConstants.NewUserPassword;

        public PlainSignUpTests(SvenWebAppFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                HandleCookies = true,
                AllowAutoRedirect = true
            });
            _userVerificationStore = _factory.Services.GetRequiredService<IStore<string, UserVerificationCode>>();
        }
        [Fact(DisplayName = "signup flow creates user and redirects to sign-in")]
        [Trait("Category", "Integration")]
        [Trait("Type", "Happy path")]
        public async Task PlainSignUp_WithValidData_CreatesUser()
        {
            HttpResponseMessage emailGetResponse = await _client.GetAsync(Endpoints.Connect.SignUp);
            Assert.Equal(HttpStatusCode.OK, emailGetResponse.StatusCode);

            // Step 1: Enter Email
            string emailGetResponseContent = await emailGetResponse.Content.ReadAsStringAsync();
            string aftEmailGet = MiscHelper.GetAntiforgeryTokenFromContent(emailGetResponseContent);
            HttpRequestMessage step1 = new(HttpMethod.Post, Endpoints.Connect.SignUp)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { AuthConstants.PropertyNames.Email, _newUserEmail },
                    { AuthConstants.PropertyNames.Step, ((int)SignUpStep.EnterEmail).ToString() },
                    { MiscHelper.AntiforgeryFormKey, aftEmailGet }
                })
            };
            HttpResponseMessage enterEmailResponse = await _client.SendAsync(step1);
            Assert.Equal(HttpStatusCode.OK, enterEmailResponse.StatusCode);
            SvenUrl urlStep2 = new SvenUrl(enterEmailResponse.RequestMessage?.RequestUri?.ToString());
            urlStep2.CheckForParameter(AuthConstants.PropertyNames.Step, SignUpStep.VerifyCode.ToCode());
            urlStep2.CheckForParameter(AuthConstants.PropertyNames.Challenge);

            // Step 2: Verify Code
            Result<UserVerificationCode> codeResult = await _userVerificationStore.GetAsync(urlStep2.GetParameterValue(AuthConstants.PropertyNames.Challenge)!);
            string enterEmailResponseContent = await enterEmailResponse.Content.ReadAsStringAsync();
            string aftEnterEmail = MiscHelper.GetAntiforgeryTokenFromContent(enterEmailResponseContent);
            HttpRequestMessage step2 = new(HttpMethod.Post, urlStep2.Url)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { AuthConstants.PropertyNames.Email, MiscHelper.GetHiddenInputValue(enterEmailResponseContent, AuthConstants.PropertyNames.Email) },
                    { AuthConstants.PropertyNames.VerificationCode, codeResult.Value.VerificationCode },
                    { AuthConstants.PropertyNames.Action, AuthConstants.Actions.VerifyCode },
                    { AuthConstants.PropertyNames.Step, ((int)SignUpStep.VerifyCode).ToString() },
                    { MiscHelper.AntiforgeryFormKey, aftEnterEmail }
                })
            };
            HttpResponseMessage verifyCodeResponse = await _client.SendAsync(step2);
            Assert.Equal(HttpStatusCode.OK, verifyCodeResponse.StatusCode);
            SvenUrl urlStep3 = new SvenUrl(verifyCodeResponse.RequestMessage?.RequestUri?.ToString());
            urlStep3.CheckForParameter(AuthConstants.PropertyNames.Step, SignUpStep.SetPassword.ToCode());
            urlStep3.CheckForParameter(AuthConstants.PropertyNames.Challenge);

            // Step 3: Set Password
            string verifyCodeResponseContent = await verifyCodeResponse.Content.ReadAsStringAsync();
            string aftVerifyCode = MiscHelper.GetAntiforgeryTokenFromContent(verifyCodeResponseContent);
            HttpRequestMessage step3 = new(HttpMethod.Post, urlStep3.Url)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { AuthConstants.PropertyNames.Email, MiscHelper.GetHiddenInputValue(verifyCodeResponseContent, AuthConstants.PropertyNames.Email) },
                { AuthConstants.PropertyNames.Password, _newUserPass },
                { AuthConstants.PropertyNames.ConfirmPassword, _newUserPass },
                { AuthConstants.PropertyNames.Step, ((int)SignUpStep.SetPassword).ToString() },
                { MiscHelper.AntiforgeryFormKey, aftVerifyCode }
            })
            };
            HttpResponseMessage setPasswordResponse = await _client.SendAsync(step3);
            Assert.Equal(HttpStatusCode.OK, setPasswordResponse.StatusCode);
        }

        [Fact(DisplayName = "signup flow fails if email already exists")]
        [Trait("Category", "Integration")]
        [Trait("Type", "Expected error")]
        public async Task PlainSignUp_ExistingEmail_ShowsError()
        {
            string existingEmail = TestDataConstants.ExistingUserEmail;

            HttpResponseMessage emailGetResponse = await _client.GetAsync(Endpoints.Connect.SignUp);
            Assert.Equal(HttpStatusCode.OK, emailGetResponse.StatusCode);

            // Step 1: Enter Email
            string emailGetResponseContent = await emailGetResponse.Content.ReadAsStringAsync();
            string aftEmailGet = MiscHelper.GetAntiforgeryTokenFromContent(emailGetResponseContent);
            HttpRequestMessage step1 = new(HttpMethod.Post, Endpoints.Connect.SignUp)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { AuthConstants.PropertyNames.Email, existingEmail },
                    { AuthConstants.PropertyNames.Step, ((int)SignUpStep.EnterEmail).ToString() },
                    { MiscHelper.AntiforgeryFormKey, aftEmailGet }
                })
            };
            HttpResponseMessage enterEmailResponse = await _client.SendAsync(step1);
            Assert.Equal(HttpStatusCode.OK, enterEmailResponse.StatusCode);
            Assert.Contains(Endpoints.Connect.SignIn, enterEmailResponse.RequestMessage?.RequestUri?.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact(DisplayName = "signup flow fails with invalid verification code")]
        public async Task PlainSignUp_InvalidCode_ShowsError()
        {
            string userEmail = "user2@example.com";
            HttpResponseMessage emailGetResponse = await _client.GetAsync(Endpoints.Connect.SignUp);
            Assert.Equal(HttpStatusCode.OK, emailGetResponse.StatusCode);

            // Step 1: Enter Email
            string emailGetResponseContent = await emailGetResponse.Content.ReadAsStringAsync();
            string aftEmailGet = MiscHelper.GetAntiforgeryTokenFromContent(emailGetResponseContent);
            HttpRequestMessage step1 = new(HttpMethod.Post, Endpoints.Connect.SignUp)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { AuthConstants.PropertyNames.Email, userEmail },
                    { AuthConstants.PropertyNames.Step, ((int)SignUpStep.EnterEmail).ToString() },
                    { MiscHelper.AntiforgeryFormKey, aftEmailGet }
                })
            };
            HttpResponseMessage enterEmailResponse = await _client.SendAsync(step1);
            Assert.Equal(HttpStatusCode.OK, enterEmailResponse.StatusCode);
            SvenUrl urlStep2 = new SvenUrl(enterEmailResponse.RequestMessage?.RequestUri?.ToString());
            urlStep2.CheckForParameter(AuthConstants.PropertyNames.Step, SignUpStep.VerifyCode.ToCode());
            urlStep2.CheckForParameter(AuthConstants.PropertyNames.Challenge);

            // Step 2: Verify Code
            string wrongVerificationCode = "-123";
            string enterEmailResponseContent = await enterEmailResponse.Content.ReadAsStringAsync();
            string aftEnterEmail = MiscHelper.GetAntiforgeryTokenFromContent(enterEmailResponseContent);
            HttpRequestMessage step2 = new(HttpMethod.Post, urlStep2.Url)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { AuthConstants.PropertyNames.Email, MiscHelper.GetHiddenInputValue(enterEmailResponseContent, AuthConstants.PropertyNames.Email) },
                    { AuthConstants.PropertyNames.VerificationCode, wrongVerificationCode },
                    { AuthConstants.PropertyNames.Action, AuthConstants.Actions.VerifyCode },
                    { AuthConstants.PropertyNames.Step, ((int)SignUpStep.VerifyCode).ToString() },
                    { MiscHelper.AntiforgeryFormKey, aftEnterEmail }
                })
            };
            HttpResponseMessage verifyCodeResponse = await _client.SendAsync(step2);
            Assert.Equal(HttpStatusCode.OK, verifyCodeResponse.StatusCode);
            string verifyCodeResponseContent = await verifyCodeResponse.Content.ReadAsStringAsync();
            Assert.Contains("Invalid verification code", verifyCodeResponseContent, StringComparison.OrdinalIgnoreCase);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!string.IsNullOrEmpty(_newUserEmail))
            {
                using (IServiceScope scope = _factory.Services.CreateScope())
                {
                    IUserProvider _userProvider = scope.ServiceProvider.GetRequiredService<IUserProvider>();
                    _userProvider.DeleteUserAsync(_newUserEmail, CancellationToken.None).GetAwaiter().GetResult();
                }
            }
            _client.Dispose();
        }
    }
}
