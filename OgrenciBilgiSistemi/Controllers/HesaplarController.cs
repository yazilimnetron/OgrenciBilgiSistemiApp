using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OgrenciBilgiSistemi.Data;
using OgrenciBilgiSistemi.Dtos;
using OgrenciBilgiSistemi.Models;
using System.Security.Claims;

namespace OgrenciBilgiSistemi.Controllers
{
    public class HesaplarController : Controller
    {
        private readonly AppDbContext _context;

        public HesaplarController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Giris() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Giris(LoginDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var user = await _context.Kullanicilar
                .Where(k => k.KullaniciDurum)
                .SingleOrDefaultAsync(u => u.KullaniciAdi == dto.KullaniciAdi);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya şifre.");
                return View(dto);
            }

            // Şifre doğrulaması
            var passwordHasher = new PasswordHasher<KullaniciModel>();
            var result = passwordHasher.VerifyHashedPassword(user, user.Sifre, dto.Sifre);
            if (result != PasswordVerificationResult.Success)
            {
                ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya şifre.");
                return View(dto);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.KullaniciAdi),
                new Claim(ClaimTypes.NameIdentifier, user.KullaniciId.ToString()),
                new Claim("userid", user.KullaniciId.ToString()),
                new Claim("KullaniciId", user.KullaniciId.ToString()),
                new Claim("sub", user.KullaniciId.ToString())
            };

            if (user.AdminMi)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = dto.BeniHatirla
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                authProperties);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Cikis()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Giris", "Hesaplar");
        }

        [AllowAnonymous]
        public IActionResult YetkisizGiris() => View();
    }
}
