using Bureau.AspNetCore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Sven.Configurations;
using Sven;
using Sven.Services;
using System.Security.Cryptography;

namespace Sven.Controllers
{
    [ApiController]
    [Route(Endpoints.WellKnown.Base)]
    public class DiscoveryController : BureauApiControllerBase
    {
        private readonly DiscoveryService _discoveryService;
        private readonly RsaSecurityKey _rsaKey;

        public DiscoveryController(ILogger<DiscoveryController> logger, DiscoveryService discoveryService, RsaSecurityKey rsaKey) : base(logger)
        {
            _discoveryService = discoveryService;
            _rsaKey = rsaKey;
        }

        [HttpGet(Endpoints.WellKnown.OpenConfigurationPath)]
        [ProducesResponseType<DiscoveryDocument>(StatusCodes.Status200OK)]
        public IActionResult GetConfiguration()
        {
            return Ok(_discoveryService.GetDiscoveryDocument());
        }

        [HttpGet(Endpoints.WellKnown.JwksPath)]
        [ProducesResponseType<Jwks>(StatusCodes.Status200OK)]
        public IActionResult GetJwks()
        {
            RSAParameters parameters = _rsaKey.Rsa.ExportParameters(false);

            Jwk jwk = new Jwk(_rsaKey.KeyId, Base64UrlEncoder.Encode(parameters.Modulus), Base64UrlEncoder.Encode(parameters.Exponent));

            return Ok(new Jwks(jwk));
        }
    }
}