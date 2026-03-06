using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OgrenciBilgiSistemi.Api.Models;
using OgrenciBilgiSistemi.Api.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OgrenciBilgiSistemi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly LoginService _loginService;
        private readonly IConfiguration _configuration;

        public AuthController(LoginService loginService, IConfiguration configuration)
        {
            _loginService = loginService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Kullanıcı adı veya şifre boş olamaz.");

            var user = await _loginService.AuthenticateAsync(request.Username, request.Password);

            if (user is null)
                return Unauthorized("Kullanıcı adı veya şifre hatalı.");

            // JWT token üret — geçerlilik süresi 8 saat (mobil uygulama ile eşleşir)
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token,
                expiresIn = 8 * 3600, // saniye cinsinden
                user = new
                {
                    user.Id,
                    user.Username,
                    user.FullName,
                    user.UnitId,
                    user.IsActive,
                    user.IsAdmin
                }
            });
        }

        /// <summary>
        /// Kullanıcı bilgilerini claim olarak içeren imzalı JWT token üretir.
        /// </summary>
        private string GenerateJwtToken(User user)
        {
            var secretKey = _configuration["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("Jwt:SecretKey yapılandırılmamış.");

            var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,        user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti,        Guid.NewGuid().ToString()),
                new Claim("userId",  user.Id.ToString()),
                new Claim("isAdmin", user.IsAdmin.ToString().ToLower())
            };

            var token = new JwtSecurityToken(
                issuer:             _configuration["Jwt:Issuer"],
                audience:           _configuration["Jwt:Audience"],
                claims:             claims,
                expires:            DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public record LoginRequest(string Username, string Password);
}
