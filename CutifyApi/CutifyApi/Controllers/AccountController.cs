using Microsoft.AspNetCore.Mvc;
using ServiceLayer.DTOs.Account;
using ServiceLayer.Services.Interfaces;

namespace CutifyApi.Controllers
{
    public class AccountController : AppController
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto register)
        {
            var result = await _accountService.Register(register);
            if (!result)
            {
                return BadRequest("Email artıq istifadə olunub.");
            }

            return Ok("Qeydiyyat uğurla tamamlandı.");
        }
        [HttpPost]
        public async Task<IActionResult> VerifyEmail([FromBody] string[] codes)
        {
            var result = await _accountService.VerifyEmailAsync(codes);
            if (!result)
            {
                return BadRequest("Kod yanlışdır və ya vaxtı bitmişdir.");
            }

            return Ok("Email təsdiqləndi.");
        }
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            var user = await _accountService.GetUserByEmailAsync(login.Email);
            if (user == null || !user.EmailConfirmed == true || user.SoftDelete)
            {
                return Unauthorized("İstifadəçi mövcud deyil və ya təsdiqlənməyib.");
            }

            bool loginSuccess = await _accountService.Login(login);
            if (loginSuccess)
            {
                var token = _accountService.GenerateJwtToken(login.Email);
                return Ok(new { token });
            }

            return Unauthorized("Email və ya şifrə yanlışdır.");
        }

    }
}
