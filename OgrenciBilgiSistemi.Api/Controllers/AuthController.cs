using Microsoft.AspNetCore.Mvc;
using OgrenciBilgiSistemi.Api.Models;
using OgrenciBilgiSistemi.Api.Services;

namespace OgrenciBilgiSistemi.Api.Controllers
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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Kullanıcı adı veya şifre boş olamaz.");

            var user = await _loginService.AuthenticateAsync(request.Username, request.Password);

            if (user is null)
                return Unauthorized("Kullanıcı adı veya şifre hatalı.");

            return Ok(new
            {
                token    = "",
                expiresIn = 0,
                user = new
                {
                    user.Id,
                    user.Username,
                    user.UnitId,
                    user.IsActive,
                    user.IsAdmin
                }
            });
        }
    }

    public record LoginRequest(string Username, string Password);
}
