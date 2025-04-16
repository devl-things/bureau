using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Sven.Controllers
{

    [ApiController]
    public class DiscoveryController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly RsaSecurityKey _rsaKey;

        public DiscoveryController(IConfiguration configuration, RsaSecurityKey rsaKey)
        {
            _configuration = configuration;
            _rsaKey = rsaKey;
        }

        [HttpGet(".well-known/openid-configuration")]
        public IActionResult GetConfiguration()
        {
            string issuer = _configuration["Jwt:Issuer"] ?? "https://localhost:5001";

            return Ok(new
            {
                issuer = issuer,
                authorization_endpoint = issuer + "/connect/authorize",
                token_endpoint = issuer + "/connect/token",
                userinfo_endpoint = issuer + "/connect/userinfo",
                jwks_uri = issuer + "/.well-known/jwks.json",
                response_types_supported = new[] { "code" },
                subject_types_supported = new[] { "public" },
                id_token_signing_alg_values_supported = new[] { "RS256" },
                token_endpoint_auth_methods_supported = new[] { "none" },
                code_challenge_methods_supported = new[] { "S256" }
            });
        }

        [HttpGet(".well-known/jwks.json")]
        public IActionResult GetJwks()
        {
            RSAParameters parameters = _rsaKey.Rsa.ExportParameters(false);

            string n = Base64UrlEncoder.Encode(parameters.Modulus);
            string e = Base64UrlEncoder.Encode(parameters.Exponent);

            object jwk = new
            {
                kty = "RSA",
                use = "sig",
                kid = _rsaKey.KeyId,
                alg = "RS256",
                n = n,
                e = e
            };

            return Ok(new { keys = new[] { jwk } });
        }
    }
}