using Bureau.Core;
using Bureau.Identity.Models;
using Bureau.Identity.Managers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Bureau.UI.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly IUserManager _userManager;
        private readonly IAccountManager _accountManager;
        private readonly IConfiguration _configuration;

        public TokenController(IUserManager userManager, IAccountManager accountManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _accountManager = accountManager;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel? model)
        {
            Result<IUserId> userResult = await _userManager.GetUserIdByNameAsync(model.Username);
            Result<bool> isPasswordValidResult = await _accountManager.IsPasswordValidAsync(userResult.Value, model.Password);
            if (userResult.IsError || isPasswordValidResult.IsError || !isPasswordValidResult.Value)
            {
                return Unauthorized();
            }

            var token = GenerateJwtToken(userResult.Value);
            return Ok(new { Token = token });
        }

        private string GenerateJwtToken(IUserId user)
        {
            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            //new Claim(ClaimTypes.Name, user.UserName)
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtIssuer"],
                audience: _configuration["JwtAudience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
