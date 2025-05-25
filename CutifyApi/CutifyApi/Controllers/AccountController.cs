using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Data;
using ServiceLayer.DTOs.Account;
using ServiceLayer.Services.Interfaces;

namespace CutifyApi.Controllers
{
    public class AccountController : AppController
    {
        private readonly IAccountService _accountService;
        private readonly AppDbContext _context;
        public AccountController(IAccountService accountService,AppDbContext context)
        {
            _accountService = accountService;
            _context = context;
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
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var tokenEntity = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken && t.ExpiryDate > DateTime.UtcNow);

            if (tokenEntity == null)
                return Unauthorized("Invalid or expired refresh token");

            // Köhnə token sil
            _context.RefreshTokens.Remove(tokenEntity);
            await _context.SaveChangesAsync();

            // Yeni tokenlər yarat
            var tokens = _accountService.GenerateTokens(tokenEntity.UserEmail);
            return Ok(tokens);
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tokenResponse = await _accountService.Login(login);

            if (tokenResponse == null)
                return Unauthorized("Email və ya şifrə yanlışdır.");

            return Ok(tokenResponse);
        }

    }
}
