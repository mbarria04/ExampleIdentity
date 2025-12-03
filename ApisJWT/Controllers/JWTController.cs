using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApisJWT.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JWTController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly UserManager<IdentityUser> _userManager;

        public JWTController(IConfiguration config)
        {
            _config = config;
          
        }

        [HttpGet("generate")]
        [AllowAnonymous] // Permite acceso sin autenticación previa
        public IActionResult GenerateToken()
        {
            // Puedes dejarlo sin claims o poner un claim genérico
            var claims = new[]
            {
        new Claim(ClaimTypes.Name, "ApiWeb2Client")
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30), // duración del token
                signingCredentials: creds);

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }

    }
}