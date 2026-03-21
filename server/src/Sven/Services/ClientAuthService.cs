using Bureau;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Sven.Configurations;
using Sven.Models;

namespace Sven.Services
{
    internal sealed class ClientAuthService : IClientAuthService
    {
        private readonly IClientService _clientService;
        private readonly ILogger<ClientAuthService> _logger;

        public ClientAuthService(IClientService clientService, ILogger<ClientAuthService> logger)
        {
            _clientService = clientService;
            _logger = logger;
        }

        public async Task<Result<Client>> AuthenticateClientAsync(
            HttpRequest request, CancellationToken cancellationToken = default)
        {
            // Step 1 — Detect which auth method(s) are present
            StringValues authorizationHeader = request.Headers.Authorization;
            bool hasBasic = !StringValues.IsNullOrEmpty(authorizationHeader)
                && authorizationHeader.ToString().StartsWith("Basic ", StringComparison.OrdinalIgnoreCase);

            StringValues clientSecretFormValue = request.HasFormContentType
                ? request.Form["client_secret"]
                : StringValues.Empty;
            bool hasPost = !StringValues.IsNullOrEmpty(clientSecretFormValue)
                && !string.IsNullOrWhiteSpace(clientSecretFormValue.ToString());

            // Step 2 — Dual-method check (RFC 6749 §2.3)
            if (hasBasic && hasPost)
            {
                return ResultError.From(
                    AuthConstants.OAuth.Errors.InvalidRequest,
                    "Multiple client authentication methods in the same request.");
            }

            // Step 3 — Extract credentials
            string clientId;
            string clientSecret;

            if (hasBasic)
            {
                string headerValue = authorizationHeader.ToString();
                if (!TryExtractBasicCredentials(headerValue, out clientId, out clientSecret))
                {
                    return ResultError.From(
                        AuthConstants.OAuth.Errors.InvalidRequest,
                        "Malformed Basic credentials.");
                }
            }
            else if (hasPost)
            {
                clientId = request.Form["client_id"].ToString();
                clientSecret = clientSecretFormValue.ToString();
            }
            else
            {
                return ResultError.From(
                    AuthConstants.OAuth.Errors.InvalidClient,
                    "No client authentication method provided.");
            }

            // Step 4 — Look up client
            Result<Client> clientResult = await _clientService.GetClientAsync(clientId, cancellationToken);
            if (clientResult.IsError)
            {
                _logger.LogWarning("Client lookup failed for client_id {ClientId}: {Error}",
                    clientId, clientResult.Error.ErrorMessage);
                return ResultError.From(
                    AuthConstants.OAuth.Errors.InvalidClient,
                    "Client not found.");
            }

            // Step 5 — Enforce confidential-only
            if (string.IsNullOrWhiteSpace(clientResult.Value.HashedSecret))
            {
                return ResultError.From(
                    AuthConstants.OAuth.Errors.InvalidClient,
                    "Client is not a confidential client.");
            }

            // Step 6 — Verify secret
            if (!PasswordHasher.VerifyPassword(clientResult.Value.HashedSecret, clientSecret))
            {
                _logger.LogWarning("Credential mismatch for client_id {ClientId}", clientId);
                return ResultError.From(
                    AuthConstants.OAuth.Errors.InvalidClient,
                    "Invalid client credentials.");
            }

            // Step 7 — Success
            return clientResult.Value;
        }

        private static bool TryExtractBasicCredentials(
            string headerValue, out string clientId, out string clientSecret)
        {
            clientId = string.Empty;
            clientSecret = string.Empty;

            if (!headerValue.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string encoded = headerValue["Basic ".Length..].Trim();
            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(encoded);
            }
            catch (FormatException)
            {
                return false;
            }

            string decoded = System.Text.Encoding.UTF8.GetString(bytes);
            int colonIndex = decoded.IndexOf(':');
            if (colonIndex < 0)
            {
                return false;
            }

            clientId = decoded[..colonIndex];
            clientSecret = decoded[(colonIndex + 1)..];
            return !string.IsNullOrWhiteSpace(clientId);
        }
    }
}
