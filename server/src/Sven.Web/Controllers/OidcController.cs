using Bureau;
using Bureau.AspNetCore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Sven.Configurations;
using Sven;
using Sven.Services;
using System.Net.Mime;

namespace Sven.Controllers
{
    [ApiController]
    [Route(Endpoints.Oidc.Base)]
    public class OidcController : BureauApiControllerBase
    {
        private readonly IClientService _clientService;
        private readonly IConfiguration _configuration;

        public OidcController(ILogger<OidcController> logger, IClientService clientService, IConfiguration configuration) : base(logger)
        {
            _clientService = clientService;
            _configuration = configuration;
        }
        [HttpPost(Endpoints.Oidc.RegisterPath)]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> RegisterAsync([FromBody] ClientRegistrationRequest request, CancellationToken cancellationToken = default)
        {
            bool isConfidentialRequest =
                AuthConstants.OAuth.TokenAuthMethods.ClientSecretBasic.Equals(request.TokenEndpointAuthMethod)
                || AuthConstants.OAuth.TokenAuthMethods.ClientSecretPost.Equals(request.TokenEndpointAuthMethod);

            if (isConfidentialRequest)
            {
                string? expectedIat = _configuration["Sven:InitialAccessToken"];
                if (string.IsNullOrWhiteSpace(expectedIat))
                {
                    return StatusCode(501, "Confidential client registration is not configured.");
                }
                if (!Request.Headers.TryGetValue("Authorization", out StringValues authHeader)
                    || !authHeader.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    || !authHeader.ToString()["Bearer ".Length..].Trim().Equals(expectedIat, StringComparison.Ordinal))
                {
                    return Unauthorized();
                }
            }

            if (!isConfidentialRequest && AreRedirectUrisInvalid(request.RedirectUris, out OAuthError? redirectUrisError))
            {
                return BadRequest(redirectUrisError);
            }
            if (!DiscoveryService.TryGetSupportedAuthMethod(request.TokenEndpointAuthMethod, out string? authMethod))
            {
                return BadRequest(new OAuthError(AuthConstants.OAuth.Errors.InvalidClientMetadata, $"Token endpoint auth method {request.TokenEndpointAuthMethod} not supported"));
            }
            if (!DiscoveryService.TryGetSupportedGrantTypes(request.GrantTypes, out List<string>? grantTypes))
            {
                return BadRequest(new OAuthError(AuthConstants.OAuth.Errors.InvalidClientMetadata, $"Grant types {string.Join(", ", request.GrantTypes!)} not supported"));
            }
            if (!DiscoveryService.TryGetSupportedResponseTypes(request.ResponseTypes, out List<string>? responseTypes))
            {
                return BadRequest(new OAuthError(AuthConstants.OAuth.Errors.InvalidClientMetadata, $"Response types {string.Join(", ", request.ResponseTypes!)} not supported"));
            }
            if (!DiscoveryService.TryGetSupportedScope(request.Scope, out string? scope))
            {
                return BadRequest(new OAuthError(AuthConstants.OAuth.Errors.InvalidClientMetadata, $"Scope {request.Scope} not supported"));
            }
            if (AreOtherUrisInvalid(request, out OAuthError? otherUrisError))
            {
                return BadRequest(otherUrisError);
            }
            ClientRequest clientRequest = new()
            {
                ClientName = request.ClientName,
                ClientUri = request.ClientUri,
                Contacts = request.Contacts,
                GrantTypes = grantTypes!,
                Jwks = request.Jwks,
                JwksUri = request.JwksUri,
                LogoUri = request.LogoUri,
                PolicyUri = request.PolicyUri,
                RedirectUris = request.RedirectUris ?? [],
                ResponseTypes = responseTypes!,
                Scope = scope!,
                SoftwareId = request.SoftwareId,
                SoftwareVersion = request.SoftwareVersion,
                TokenEndpointAuthMethod = authMethod!,
                TosUri = request.TosUri
            };
            Result<Client> newClientResult = await _clientService.CreateClientAsync(clientRequest, cancellationToken);
            if (newClientResult.IsError)
            {
                return ProblemDetailsResponse(newClientResult.Error);
            }
            return Created(uri: $"{Endpoints.Oidc.Register}", ToClientRegistrationResponse(newClientResult.Value));
        }

        private static ClientRegistrationResponse ToClientRegistrationResponse(Client client)
        {
            return new()
            {
                ClientId = client.Identifier,
                ClientIdIssuedAt = client.CreatedAt,
                ClientName = client.Name,
                ClientSecret = client.ClientSecret,
                ClientSecretExpiresAt = client.ClientSecretExpiresAt,
                ClientUri = client.ClientUri,
                Contacts = client.Contacts,
                GrantTypes = client.GrantTypes,
                Jwks = client.Jwks,
                JwksUri = client.JwksUri,
                LogoUri = client.LogoUri,
                PolicyUri = client.PolicyUri,
                RedirectUris = [.. client.RedirectUris],
                ResponseTypes = client.ResponseTypes,
                Scope = client.Scope.Scope,
                SoftwareId = client.SoftwareId,
                SoftwareVersion = client.SoftwareVersion,
                TokenEndpointAuthMethod = client.AuthMethod,
                TosUri = client.TosUri
            };
        }

        private static bool AreRedirectUrisInvalid(List<string>? redirectUris, out OAuthError? validationError)
        {
            validationError = null;
            if (redirectUris == null || redirectUris.Count == 0)
            {
                validationError = new OAuthError(AuthConstants.OAuth.Errors.InvalidRedirectUri, $"Redirect Uris are missing.");
                return true;
            }
            foreach (string redirectUri in redirectUris)
            {
                if (!UriValidator.IsRedirectUriValid(redirectUri))
                {
                    validationError = new OAuthError(AuthConstants.OAuth.Errors.InvalidRedirectUri, $"Redirect uri {redirectUri} invalid.");
                    return true;
                }
            }
            return false;
        }

        private static bool AreOtherUrisInvalid(ClientRegistrationRequest request, out OAuthError? validationError)
        {
            if (IsInvalidUri(request.ClientUri, nameof(request.ClientUri), out validationError)) return true;
            if (IsInvalidUri(request.JwksUri, nameof(request.JwksUri), out validationError)) return true;
            if (IsInvalidUri(request.LogoUri, nameof(request.LogoUri), out validationError)) return true;
            if (IsInvalidUri(request.PolicyUri, nameof(request.PolicyUri), out validationError)) return true;
            if (IsInvalidUri(request.TosUri, nameof(request.TosUri), out validationError)) return true;
            validationError = null;
            return false;
        }

        private static bool IsInvalidUri(string? uri, string field, out OAuthError? validationError)
        {
            if (!string.IsNullOrWhiteSpace(uri) && !UriValidator.IsUriValid(uri))
            {
                validationError = new OAuthError(AuthConstants.OAuth.Errors.InvalidClientMetadata, $"{field} {uri} invalid.");
                return true;
            }
            validationError = null;
            return false;
        }
    }
}
