using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sven.Configurations;
using Sven.Models;
using System.Security.Cryptography;

namespace Sven.Controllers
{
    [ApiController]
    [Route(Endpoints.WellKnown.Base)]
    public class DiscoveryController : ControllerBase
    {
        private readonly JwtOptions _jwtOptions;
        private readonly RsaSecurityKey _rsaKey;

        public DiscoveryController(IOptions<JwtOptions> jwtOptions, RsaSecurityKey rsaKey)
        {
            _jwtOptions = jwtOptions.Value;
            _rsaKey = rsaKey;
        }

        [HttpGet(Endpoints.WellKnown.OpenConfigurationPath)]
        public IActionResult GetConfiguration()
        {
            DiscoveryDocument document = new DiscoveryDocument()
            {
                Issuer = _jwtOptions.Issuer,
                AuthorizationEndpoint = $"{_jwtOptions.Issuer}{Endpoints.Connect.Authorize}",
                TokenEndpoint = $"{_jwtOptions.Issuer}{Endpoints.Connect.Token}",
                UserInfoEndpoint = $"{_jwtOptions.Issuer}{Endpoints.Oidc.UserInfo}",
                JwksUri = $"{_jwtOptions.Issuer}{Endpoints.WellKnown.Jwks}",
            };

            return Ok(document);
        }

        [HttpGet(Endpoints.WellKnown.JwksPath)]
        public IActionResult GetJwks()
        {
            RSAParameters parameters = _rsaKey.Rsa.ExportParameters(false);

            object jwk = new
            {
                kty = AuthConstants.OAuth.SigningAlgorithms.RSA,
                use = "sig",
                kid = _rsaKey.KeyId,
                alg = AuthConstants.OAuth.SigningAlgorithms.Rsa256,
                n = Base64UrlEncoder.Encode(parameters.Modulus),
                e = Base64UrlEncoder.Encode(parameters.Exponent)
            };

            return Ok(new { keys = new[] { jwk } });
        }
    }
}