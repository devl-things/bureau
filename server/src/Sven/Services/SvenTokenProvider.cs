using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sven.Configurations;
using Sven.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Sven.Services
{
    public class SvenTokenProvider : ITokenProvider
    {
        private readonly JwtOptions _jwtOptions;
        private readonly RsaSecurityKey _rsaKey;
        public SvenTokenProvider(IOptions<JwtOptions> jwtOptions, RsaSecurityKey rsaKey)
        {
            _jwtOptions = jwtOptions.Value;
            _rsaKey = rsaKey;
        }
        SvenToken ITokenProvider.CreateToken(List<Claim> claims)
        {
            SigningCredentials creds = new SigningCredentials(_rsaKey, SecurityAlgorithms.RsaSha256);
            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );


            string jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return new SvenToken(jwt);
        }
    }
}
