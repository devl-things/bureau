using Microsoft.AspNetCore.Mvc.Testing;
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
    public class ForgotSignUpTests : IClassFixture<SvenWebAppFactory>, IDisposable
    {
        private readonly SvenWebAppFactory _factory;
        private readonly HttpClient _client;
        private readonly string _existingEmail = TestDataConstants.ExistingUserEmail;

        public ForgotSignUpTests(SvenWebAppFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                HandleCookies = true,
                AllowAutoRedirect = true
            });
        }
        [Fact(DisplayName = "forgot password flow updates user password")]
        [Trait("Category", "Integration")]
        [Trait("Type", "Happy path")]
        public async Task ForgotSignUp_WithValidData_UpdatePassword()
        {
            HttpResponseMessage emailGetResponse = await _client.GetAsync(Endpoints.Connect.ForgotPassword);
            Assert.Equal(HttpStatusCode.OK, emailGetResponse.StatusCode);

            // Step 1: Enter Email
            string emailGetResponseContent = await emailGetResponse.Content.ReadAsStringAsync();
            string aftEmailGet = MiscHelper.GetAntiforgeryTokenFromContent(emailGetResponseContent);
            HttpRequestMessage step1 = new(HttpMethod.Post, Endpoints.Connect.ForgotPassword)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { AuthConstants.PropertyNames.Email, _existingEmail },
                    { AuthConstants.PropertyNames.Step, ((int)SignUpStep.EnterEmail).ToString() },
                    { MiscHelper.AntiforgeryFormKey, aftEmailGet }
                })
            };
            HttpResponseMessage enterEmailResponse = await _client.SendAsync(step1);
            Assert.Equal(HttpStatusCode.OK, enterEmailResponse.StatusCode);
            SvenUrl responseUrl = new SvenUrl(enterEmailResponse.RequestMessage?.RequestUri?.ToString());
            responseUrl.CheckForParameter(AuthConstants.PropertyNames.Step, SignUpStep.CodeSent.ToCode());
            responseUrl.CheckForParameter(AuthConstants.PropertyNames.Challenge);

            // Step 2: get the reset link from email
            List<TestLoggerProvider.LogEntry> logs = _factory.LoggerProvider.Logs;

            TestLoggerProvider.LogEntry? log = logs.FirstOrDefault(l =>
                l.Category.Contains(nameof(EmailNotificationService<PasswordResetNotification>)) &&
                l.Message.Contains("Verification code") &&
                l.Message.Contains("sent to"));

            Assert.NotNull(log);
            string? resetLink = MiscHelper.ExtractWholeUrlStartingWith(log.Message, Endpoints.Connect.ForgotPassword);
            Assert.NotNull(resetLink);

            // Step 3: simulate click on link in email
            HttpResponseMessage setPasswordGetResponse = await _client.GetAsync(resetLink);
            Assert.Equal(HttpStatusCode.OK, setPasswordGetResponse.StatusCode);

            // Step 4: enter new password
            string setPasswordGetResponseContent = await setPasswordGetResponse.Content.ReadAsStringAsync();
            string aftSetPassword = MiscHelper.GetAntiforgeryTokenFromContent(setPasswordGetResponseContent);
            HttpRequestMessage step3 = new(HttpMethod.Post, resetLink)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { AuthConstants.PropertyNames.Email, MiscHelper.GetHiddenInputValue(setPasswordGetResponseContent, AuthConstants.PropertyNames.Email) },
                    { AuthConstants.PropertyNames.Password, TestDataConstants.NewUserPassword },
                    { AuthConstants.PropertyNames.ConfirmPassword, TestDataConstants.NewUserPassword },
                    { AuthConstants.PropertyNames.Step, ((int)SignUpStep.SetPassword).ToString() },
                    { MiscHelper.AntiforgeryFormKey, aftSetPassword }
                })
            };
            HttpResponseMessage setPasswordResponse = await _client.SendAsync(step3);
            Assert.Equal(HttpStatusCode.OK, setPasswordResponse.StatusCode);
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
