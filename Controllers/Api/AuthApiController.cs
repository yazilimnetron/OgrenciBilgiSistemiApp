using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OgrenciBilgiSistemi.Data;
using OgrenciBilgiSistemi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OgrenciBilgiSistemi.Controllers.Api
{
    [ApiController]
    [Route("api/auth")]
    [AllowAnonymous]
    public class AuthApiController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AuthApiController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.KullaniciAdi) || string.IsNullOrWhiteSpace(req.Sifre))
                return BadRequest(new { hata = "Kullanıcı adı ve şifre zorunludur." });

            var user = await _db.Kullanicilar
                .Where(k => k.KullaniciDurum)
                .SingleOrDefaultAsync(u => u.KullaniciAdi == req.KullaniciAdi, ct);

            if (user == null)
                return Unauthorized(new { hata = "Geçersiz kullanıcı adı veya şifre." });

            var hasher = new PasswordHasher<KullaniciModel>();
            var result = hasher.VerifyHashedPassword(user, user.Sifre, req.Sifre);

            if (result != PasswordVerificationResult.Success)
                return Unauthorized(new { hata = "Geçersiz kullanıcı adı veya şifre." });

            var token = OlusturToken(user);

            return Ok(new LoginResponse
            {
                Token = token,
                KullaniciAdi = user.KullaniciAdi,
                KullaniciId = user.KullaniciId,
                AdminMi = user.AdminMi,
                GecerlilikSuresi = DateTime.UtcNow.AddHours(GetAccessTokenHours())
            });
        }

        // POST /api/auth/token-dogrula
        [HttpPost("token-dogrula")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult TokenDogrula()
        {
            var kullaniciAdi = User.FindFirstValue(ClaimTypes.Name);
            var kullaniciId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var adminMi = User.IsInRole("Admin");

            return Ok(new { gecerli = true, kullaniciAdi, kullaniciId, adminMi });
        }

        // -------------------------------------------------------
        private string OlusturToken(KullaniciModel user)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSection["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.KullaniciAdi),
                new(ClaimTypes.NameIdentifier, user.KullaniciId.ToString()),
                new("KullaniciId", user.KullaniciId.ToString()),
                new(JwtRegisteredClaimNames.Sub, user.KullaniciId.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (user.AdminMi)
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(GetAccessTokenHours()),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private int GetAccessTokenHours()
            => int.TryParse(_config["Jwt:AccessTokenExpiresHours"], out var h) ? h : 8;
    }

    public sealed class LoginRequest
    {
        public string KullaniciAdi { get; set; } = string.Empty;
        public string Sifre { get; set; } = string.Empty;
    }

    public sealed class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string KullaniciAdi { get; set; } = string.Empty;
        public int KullaniciId { get; set; }
        public bool AdminMi { get; set; }
        public DateTime GecerlilikSuresi { get; set; }
    }
}
