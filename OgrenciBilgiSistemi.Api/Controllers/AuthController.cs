using Microsoft.AspNetCore.Mvc;
using StudentTrackingSystem.Api.Models;
using StudentTrackingSystem.Api.Services;

namespace StudentTrackingSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly LoginService _loginService;

        public AuthController(LoginService loginService)
        {
            _loginService = loginService;
        }
        /*
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User loginRequest)
        {
            if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Username))
                return BadRequest("Geçersiz istek.");

            var user = await _loginService.AuthenticateAsync(loginRequest.Username, loginRequest.Password);

            if (user != null)
            {
                // Başarılı giriş: Kullanıcı nesnesini döndür
                return Ok(user);
            }

            return Unauthorized("Kullanıcı adı veya şifre hatalı.");
        }*/
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User loginRequest)
        {
            // Gelen veriyi loglayarak kontrol et (Debug penceresinden bakabilirsin)
            if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Username))
                return BadRequest("Kullanıcı adı veya şifre boş geldi.");

            var user = await _loginService.AuthenticateAsync(loginRequest.Username, loginRequest.Password);

            if (user != null)
                return Ok(user);

            return Unauthorized("Kullanıcı bulunamadı.");
        }
    }
}