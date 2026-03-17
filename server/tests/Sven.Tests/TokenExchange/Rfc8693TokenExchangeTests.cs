using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Sven.Configurations;
using Sven.Data;
using Sven.Data.Models;
using Sven.Data.Repositories;
using Sven.Models;
using Sven.Services;
using Sven.Tests.Fixtures;
using Sven.Tests.TestData;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Sven.Tests.TokenExchange
{
    [Trait("Category", "TokenExchange")]
    public class Rfc8693TokenExchangeTests : IClassFixture<SvenWebAppFactory>, IDisposable
    {
        private readonly SvenWebAppFactory _factory;
        private readonly HttpClient _client;

        private static readonly string HashedSecret = PasswordHasher.HashPassword(TestDataConstants.TestConfidentialClientSecret);
        private const string TestExchangeClientId = "test-exchange-client";
        private const string TestProvider = "google";
        private const string TestFeatureKey = "watson.calendar";
        private const string TestUserId = "exchange-test-user-01";
        private const string TestRedirectUri = "https://localhost:3000/callback";

        public Rfc8693TokenExchangeTests(SvenWebAppFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Seeding helpers
        // ─────────────────────────────────────────────────────────────────────────

        private async Task SeedExchangeClientAsync(Dictionary<string, List<string>>? bureauFeatures = null)
        {
            Client exchangeClient = new Client
            {
                Identifier = TestExchangeClientId,
                Name = "Test Token Exchange Client",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Active = true,
                HashedSecret = HashedSecret,
                Type = AuthConstants.ClientTypes.Confidential,
                RedirectUris = new HashSet<string> { TestRedirectUri },
                Scope = new ScopeParameter(TestFeatureKey),
                BureauFeatures = bureauFeatures,
            };

            using IServiceScope serviceScope = _factory.Services.CreateScope();
            IClientRepository store = serviceScope.ServiceProvider.GetRequiredService<IClientRepository>();
            await store.StoreAsync(exchangeClient, CancellationToken.None);
        }

        private async Task SeedUserExternalTokenAsync(string userId, string provider = TestProvider)
        {
            UserExternalToken token = new UserExternalToken
            {
                UserId = userId,
                Provider = provider,
                ExternalAccountId = $"{provider}-account-{userId}",
                Scopes = "calendar.read",
                AccessToken = $"external-access-token-for-{userId}",
                RefreshToken = null,
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
                LinkedAt = DateTimeOffset.UtcNow,
                RequiresReauthorisation = false,
            };

            using IServiceScope serviceScope = _factory.Services.CreateScope();
            IExternalTokenService externalTokenService = serviceScope.ServiceProvider.GetRequiredService<IExternalTokenService>();
            await externalTokenService.StoreAsync(token, CancellationToken.None);
        }

        private async Task SeedHouseholdMemberAsync(string userId, string householdId, string role = "member")
        {
            using IServiceScope serviceScope = _factory.Services.CreateScope();
            SvenTestContext context = serviceScope.ServiceProvider.GetRequiredService<SvenTestContext>();

            bool householdExists = context.Households.Any(h => h.Identifier == householdId);
            if (!householdExists)
            {
                context.Households.Add(new HouseholdDb
                {
                    Identifier = householdId,
                    Name = "Test Household",
                    CreatedAt = DateTimeOffset.UtcNow,
                });
            }

            context.HouseholdMembers.Add(new HouseholdMemberDb
            {
                HouseholdIdentifier = householdId,
                UserId = userId,
                Role = role,
                JoinedAt = DateTimeOffset.UtcNow,
            });

            await context.SaveChangesAsync(CancellationToken.None);
        }

        private async Task SeedSharedExternalTokenAsync(string ownerUserId, string householdId, string provider = TestProvider, string featureKey = TestFeatureKey)
        {
            using IServiceScope serviceScope = _factory.Services.CreateScope();
            SvenTestContext context = serviceScope.ServiceProvider.GetRequiredService<SvenTestContext>();

            context.SharedExternalTokens.Add(new SharedExternalTokenDb
            {
                Identifier = Guid.NewGuid().ToString("N"),
                HouseholdIdentifier = householdId,
                OwnerUserId = ownerUserId,
                Provider = provider,
                FeatureKey = featureKey,
                SharedAt = DateTimeOffset.UtcNow,
                RevokedAt = null,
            });

            await context.SaveChangesAsync(CancellationToken.None);
        }

        // Build a valid Sven JWT for a given user and scope (signed with the server's RSA key).
        private string BuildValidSubjectToken(string userId, string scope)
        {
            RsaSecurityKey serverKey = _factory.Services.GetRequiredService<RsaSecurityKey>();
            SigningCredentials creds = new SigningCredentials(serverKey, SecurityAlgorithms.RsaSha256);
            JwtSecurityToken jwt = new JwtSecurityToken(
                issuer: "https://test.localhost",
                claims: new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, userId),
                    new Claim("scope", scope),
                },
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        // Build an expired Sven JWT (exp in the past, server-signed).
        private string BuildExpiredSubjectToken(string userId, string scope)
        {
            RsaSecurityKey serverKey = _factory.Services.GetRequiredService<RsaSecurityKey>();
            SigningCredentials creds = new SigningCredentials(serverKey, SecurityAlgorithms.RsaSha256);
            JwtSecurityToken jwt = new JwtSecurityToken(
                issuer: "https://test.localhost",
                claims: new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, userId),
                    new Claim("scope", scope),
                },
                expires: DateTime.UtcNow.AddHours(-1),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        private static string BuildBasicAuthHeader(string clientId, string secret)
        {
            string credentials = $"{clientId}:{secret}";
            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
            return $"Basic {encoded}";
        }

        // Posts a token exchange request using Basic auth.
        private async Task<HttpResponseMessage> PostTokenExchangeAsync(
            string clientId,
            string clientSecret,
            string subjectToken,
            string resource,
            string scope)
        {
            Dictionary<string, string> formData = new Dictionary<string, string>
            {
                [AuthConstants.OAuth.FieldNames.GrantTypeField] = AuthConstants.OAuth.GrantTypes.TokenExchange,
                [AuthConstants.OAuth.FieldNames.ClientId] = clientId,
                ["subject_token"] = subjectToken,
                ["subject_token_type"] = "urn:ietf:params:oauth:token-type:access_token",
                ["resource"] = resource,
                [AuthConstants.OAuth.FieldNames.Scope] = scope,
            };

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Endpoints.Connect.Token);
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(
                BuildBasicAuthHeader(clientId, clientSecret));
            request.Content = new FormUrlEncodedContent(formData);

            return await _client.SendAsync(request);
        }

        // Issues a Sven access token for a user via the auth code flow.
        // Seeds user, client, and auth code; POSTs to /connect/token.
        private async Task<string> ObtainSvenAccessTokenViaAuthCodeAsync(string userId, string clientId, string scope)
        {
            // Seed the public client for the auth code flow
            Client authClient = new Client
            {
                Identifier = clientId,
                Name = "Auth Code Test Client",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Active = true,
                HashedSecret = null,
                Type = AuthConstants.ClientTypes.Public,
                RedirectUris = new HashSet<string> { TestRedirectUri },
                Scope = new ScopeParameter(scope),
            };

            using (IServiceScope seedScope = _factory.Services.CreateScope())
            {
                IClientRepository clientRepo = seedScope.ServiceProvider.GetRequiredService<IClientRepository>();
                await clientRepo.StoreAsync(authClient, CancellationToken.None);
            }

            // Build PKCE code verifier and challenge
            string codeVerifier = "testcodeverifier01234567890abcdefghijklmno";
            string codeChallenge = AuthCode.GenerateSha256CodeChallenge(codeVerifier);

            // Seed an auth code directly via IAuthCodeService (bypasses Razor Pages login)
            OAuthRequest oauthRequest = new OAuthRequest
            {
                ClientId = clientId,
                RedirectUri = TestRedirectUri,
                Scope = scope,
                CodeChallenge = codeChallenge,
                CodeChallengeMethod = AuthConstants.OAuth.CodeChallengeMethods.Sha256,
            };

            List<Claim> userClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Name, "Test User"),
                new Claim(JwtRegisteredClaimNames.PreferredUsername, "testuser"),
            };

            string authCode;
            using (IServiceScope codeScope = _factory.Services.CreateScope())
            {
                IAuthCodeService authCodeService = codeScope.ServiceProvider.GetRequiredService<IAuthCodeService>();
                Bureau.Result<string> codeResult = await authCodeService.CreateAuthCodeAsync(oauthRequest, userClaims, CancellationToken.None);
                authCode = codeResult.Value;
            }

            // POST /connect/token with authorization_code grant
            Dictionary<string, string> formData = new Dictionary<string, string>
            {
                [AuthConstants.OAuth.FieldNames.GrantTypeField] = AuthConstants.OAuth.GrantTypes.AuthorizationCode,
                [AuthConstants.OAuth.FieldNames.ClientId] = clientId,
                [AuthConstants.OAuth.FieldNames.Code] = authCode,
                [AuthConstants.OAuth.FieldNames.CodeVerifier] = codeVerifier,
                [AuthConstants.OAuth.FieldNames.RedirectUri] = TestRedirectUri,
            };

            HttpResponseMessage tokenResponse = await _client.PostAsync(
                Endpoints.Connect.Token,
                new FormUrlEncodedContent(formData));

            string tokenContent = await tokenResponse.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(tokenContent);
            return doc.RootElement.GetProperty("access_token").GetString()!;
        }

        // ─────────────────────────────────────────────────────────────────────────
        // PROT-03: Token Exchange six-step validation
        // ─────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task HappyPath_ValidExchange_ReturnsExternalToken()
        {
            // PROT-03 happy path: valid client, bureau_features includes feature, valid subject_token with feature scope,
            // UserExternalToken exists for user + provider → 200 with access_token
            Dictionary<string, List<string>> bureauFeatures = new Dictionary<string, List<string>>
            {
                [TestFeatureKey] = new List<string>()
            };
            await SeedExchangeClientAsync(bureauFeatures);
            await SeedUserExternalTokenAsync(TestUserId);

            string subjectToken = BuildValidSubjectToken(TestUserId, TestFeatureKey);

            HttpResponseMessage response = await PostTokenExchangeAsync(
                TestExchangeClientId,
                TestDataConstants.TestConfidentialClientSecret,
                subjectToken,
                TestProvider,
                TestFeatureKey);

            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using JsonDocument doc = JsonDocument.Parse(content);
            Assert.True(doc.RootElement.TryGetProperty("access_token", out _),
                $"Response should contain access_token. Body: {content}");
        }

        [Fact]
        public async Task UnknownClient_ReturnsInvalidClient()
        {
            // PROT-03 step 1 failure: client_id does not exist → 401 invalid_client
            string subjectToken = BuildValidSubjectToken(TestUserId, TestFeatureKey);

            HttpResponseMessage response = await PostTokenExchangeAsync(
                "nonexistent-client-xyz",
                TestDataConstants.TestConfidentialClientSecret,
                subjectToken,
                TestProvider,
                TestFeatureKey);

            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Contains(AuthConstants.OAuth.Errors.InvalidClient, content);
        }

        [Fact]
        public async Task FeatureNotAllowed_ReturnsInvalidScope()
        {
            // PROT-03 step 2 failure: client exists but watson.calendar not in bureau_features → 400 invalid_scope
            // Seed client with empty bureau_features (no watson.calendar key)
            await SeedExchangeClientAsync(new Dictionary<string, List<string>>());

            string subjectToken = BuildValidSubjectToken(TestUserId, TestFeatureKey);

            HttpResponseMessage response = await PostTokenExchangeAsync(
                TestExchangeClientId,
                TestDataConstants.TestConfidentialClientSecret,
                subjectToken,
                TestProvider,
                TestFeatureKey);

            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(AuthConstants.OAuth.Errors.InvalidScope, content);
        }

        [Fact]
        public async Task InvalidSubjectToken_ReturnsInvalidGrant()
        {
            // PROT-03 step 3 failure: subject_token is garbage (not a valid JWT) → 400 invalid_grant
            Dictionary<string, List<string>> bureauFeatures = new Dictionary<string, List<string>>
            {
                [TestFeatureKey] = new List<string>()
            };
            await SeedExchangeClientAsync(bureauFeatures);

            HttpResponseMessage response = await PostTokenExchangeAsync(
                TestExchangeClientId,
                TestDataConstants.TestConfidentialClientSecret,
                "garbage-not-a-jwt",
                TestProvider,
                TestFeatureKey);

            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(AuthConstants.OAuth.Errors.InvalidGrant, content);
        }

        [Fact]
        public async Task ExpiredSubjectToken_ReturnsInvalidGrant()
        {
            // PROT-03 step 3 failure: subject_token is expired (server-signed, exp in past) → 400 invalid_grant
            Dictionary<string, List<string>> bureauFeatures = new Dictionary<string, List<string>>
            {
                [TestFeatureKey] = new List<string>()
            };
            await SeedExchangeClientAsync(bureauFeatures);

            string expiredToken = BuildExpiredSubjectToken(TestUserId, TestFeatureKey);

            HttpResponseMessage response = await PostTokenExchangeAsync(
                TestExchangeClientId,
                TestDataConstants.TestConfidentialClientSecret,
                expiredToken,
                TestProvider,
                TestFeatureKey);

            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(AuthConstants.OAuth.Errors.InvalidGrant, content);
        }

        [Fact]
        public async Task FeatureNotInUserScope_ReturnsInvalidGrant()
        {
            // PROT-03 step 4 failure: valid JWT but scope does not contain the feature key → 400 invalid_grant
            Dictionary<string, List<string>> bureauFeatures = new Dictionary<string, List<string>>
            {
                [TestFeatureKey] = new List<string>()
            };
            await SeedExchangeClientAsync(bureauFeatures);

            // Build JWT with a different scope (not watson.calendar)
            string subjectToken = BuildValidSubjectToken(TestUserId, "openid profile");

            HttpResponseMessage response = await PostTokenExchangeAsync(
                TestExchangeClientId,
                TestDataConstants.TestConfidentialClientSecret,
                subjectToken,
                TestProvider,
                TestFeatureKey);

            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(AuthConstants.OAuth.Errors.InvalidGrant, content);
        }

        [Fact]
        public async Task NoTokenAvailable_ReturnsInvalidGrant()
        {
            // PROT-03 step 5 failure: client OK, subject_token valid, feature in scope,
            // but no UserExternalToken and no household → 400 invalid_grant
            Dictionary<string, List<string>> bureauFeatures = new Dictionary<string, List<string>>
            {
                [TestFeatureKey] = new List<string>()
            };
            await SeedExchangeClientAsync(bureauFeatures);
            // Do NOT seed a UserExternalToken for this user

            string userId = "no-token-user-01";
            string subjectToken = BuildValidSubjectToken(userId, TestFeatureKey);

            HttpResponseMessage response = await PostTokenExchangeAsync(
                TestExchangeClientId,
                TestDataConstants.TestConfidentialClientSecret,
                subjectToken,
                TestProvider,
                TestFeatureKey);

            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains(AuthConstants.OAuth.Errors.InvalidGrant, content);
        }

        // ─────────────────────────────────────────────────────────────────────────
        // VAULT-03: Household claims in auth code flow JWT
        // ─────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task HouseholdClaims_MemberToken_IncludesHouseholdIdAndRole()
        {
            // VAULT-03 SC1: user is a household member → issued JWT contains household_id and household_role
            string userId = "household-claims-member-01";
            string householdId = "household-alpha-01";
            string clientId = "vault03-member-client";
            string scope = AuthConstants.Scopes.OpenId + " " + AuthConstants.Scopes.Profile;

            await SeedHouseholdMemberAsync(userId, householdId, "owner");

            string accessToken = await ObtainSvenAccessTokenViaAuthCodeAsync(userId, clientId, scope);

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwt = handler.ReadJwtToken(accessToken);

            System.Security.Claims.Claim? householdIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "household_id");
            System.Security.Claims.Claim? householdRoleClaim = jwt.Claims.FirstOrDefault(c => c.Type == "household_role");

            Assert.NotNull(householdIdClaim);
            Assert.NotNull(householdRoleClaim);
            Assert.Equal(householdId, householdIdClaim.Value);
            Assert.Equal("owner", householdRoleClaim.Value);
        }

        [Fact]
        public async Task NoHouseholdClaims_SoloUser_ClaimsAbsent()
        {
            // VAULT-03 SC2: user is not a household member → issued JWT does NOT contain household_id or household_role
            string userId = "solo-user-no-household-01";
            string clientId = "vault03-solo-client";
            string scope = AuthConstants.Scopes.OpenId + " " + AuthConstants.Scopes.Profile;
            // Do NOT seed any HouseholdMember row for this user

            string accessToken = await ObtainSvenAccessTokenViaAuthCodeAsync(userId, clientId, scope);

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwt = handler.ReadJwtToken(accessToken);

            Assert.DoesNotContain(jwt.Claims, c => c.Type == "household_id");
            Assert.DoesNotContain(jwt.Claims, c => c.Type == "household_role");
        }

        // ─────────────────────────────────────────────────────────────────────────
        // VAULT-04: Token exchange response shape
        // ─────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SoloUserResponse_HasAccessTokenNoBureauTokens()
        {
            // VAULT-04 SC1: user has own external token, no household → response has access_token, bureau_tokens absent
            string userId = "vault04-solo-user-01";
            Dictionary<string, List<string>> bureauFeatures = new Dictionary<string, List<string>>
            {
                [TestFeatureKey] = new List<string>()
            };
            await SeedExchangeClientAsync(bureauFeatures);
            await SeedUserExternalTokenAsync(userId);
            // Do NOT seed household membership

            string subjectToken = BuildValidSubjectToken(userId, TestFeatureKey);

            HttpResponseMessage response = await PostTokenExchangeAsync(
                TestExchangeClientId,
                TestDataConstants.TestConfidentialClientSecret,
                subjectToken,
                TestProvider,
                TestFeatureKey);

            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using JsonDocument doc = JsonDocument.Parse(content);
            Assert.True(doc.RootElement.TryGetProperty("access_token", out _),
                $"Response should contain access_token. Body: {content}");
            Assert.False(doc.RootElement.TryGetProperty("bureau_tokens", out _),
                $"Response should NOT contain bureau_tokens for solo user. Body: {content}");
        }

        [Fact]
        public async Task HouseholdAggregation_HasAccessTokenAndBureauTokens()
        {
            // VAULT-04 SC2: user has own external token + household membership + other member's shared token
            // → response has access_token and bureau_tokens with one entry
            string userId = "vault04-household-owner-01";
            string householdId = "household-vault04-agg-01";
            string otherUserId = "vault04-other-member-01";

            Dictionary<string, List<string>> bureauFeatures = new Dictionary<string, List<string>>
            {
                [TestFeatureKey] = new List<string>()
            };
            await SeedExchangeClientAsync(bureauFeatures);
            await SeedUserExternalTokenAsync(userId);
            await SeedUserExternalTokenAsync(otherUserId);
            await SeedHouseholdMemberAsync(userId, householdId, "owner");
            await SeedHouseholdMemberAsync(otherUserId, householdId, "member");
            await SeedSharedExternalTokenAsync(otherUserId, householdId);

            string subjectToken = BuildValidSubjectToken(userId, TestFeatureKey);

            HttpResponseMessage response = await PostTokenExchangeAsync(
                TestExchangeClientId,
                TestDataConstants.TestConfidentialClientSecret,
                subjectToken,
                TestProvider,
                TestFeatureKey);

            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using JsonDocument doc = JsonDocument.Parse(content);
            Assert.True(doc.RootElement.TryGetProperty("access_token", out _),
                $"Response should contain access_token. Body: {content}");
            Assert.True(doc.RootElement.TryGetProperty("bureau_tokens", out JsonElement bureauTokens),
                $"Response should contain bureau_tokens. Body: {content}");
            Assert.Equal(JsonValueKind.Array, bureauTokens.ValueKind);
            Assert.True(bureauTokens.GetArrayLength() >= 1,
                $"bureau_tokens should have at least one entry. Body: {content}");

            // Verify source_user_id matches the other member
            bool hasCorrectSourceUser = false;
            foreach (JsonElement entry in bureauTokens.EnumerateArray())
            {
                if (entry.TryGetProperty("source_user_id", out JsonElement sourceUserId)
                    && sourceUserId.GetString() == otherUserId)
                {
                    hasCorrectSourceUser = true;
                    break;
                }
            }
            Assert.True(hasCorrectSourceUser, $"bureau_tokens should contain entry with source_user_id={otherUserId}. Body: {content}");
        }

        [Fact]
        public async Task HouseholdFallback_NoOwnToken_HasBureauTokensOnly()
        {
            // VAULT-04 SC3: no own external token + household membership + other member's shared token
            // → response has no access_token, bureau_tokens has one entry
            string userId = "vault04-fallback-user-01";
            string householdId = "household-vault04-fallback-01";
            string tokenOwnerUserId = "vault04-fallback-token-owner-01";

            Dictionary<string, List<string>> bureauFeatures = new Dictionary<string, List<string>>
            {
                [TestFeatureKey] = new List<string>()
            };
            await SeedExchangeClientAsync(bureauFeatures);
            // Do NOT seed own token for userId
            await SeedUserExternalTokenAsync(tokenOwnerUserId);
            await SeedHouseholdMemberAsync(userId, householdId, "member");
            await SeedHouseholdMemberAsync(tokenOwnerUserId, householdId, "owner");
            await SeedSharedExternalTokenAsync(tokenOwnerUserId, householdId);

            string subjectToken = BuildValidSubjectToken(userId, TestFeatureKey);

            HttpResponseMessage response = await PostTokenExchangeAsync(
                TestExchangeClientId,
                TestDataConstants.TestConfidentialClientSecret,
                subjectToken,
                TestProvider,
                TestFeatureKey);

            string content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using JsonDocument doc = JsonDocument.Parse(content);
            Assert.False(doc.RootElement.TryGetProperty("access_token", out _),
                $"Response should NOT contain access_token when user has no own token. Body: {content}");
            Assert.True(doc.RootElement.TryGetProperty("bureau_tokens", out JsonElement bureauTokens),
                $"Response should contain bureau_tokens. Body: {content}");
            Assert.Equal(JsonValueKind.Array, bureauTokens.ValueKind);
            Assert.True(bureauTokens.GetArrayLength() >= 1,
                $"bureau_tokens should have at least one entry. Body: {content}");
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
