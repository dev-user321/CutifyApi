using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepositoryLayer.Data;
using ServiceLayer.DTOs.Account;
using ServiceLayer.Services.Interfaces;

namespace CutifyApi.Controllers
{
    public class AccountController : AppController
    {
        private readonly IAccountService _accountService;
        private readonly AppDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAccountService accountService, AppDbContext context, ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto register)
        {
            _logger.LogInformation("Yeni istifadəçi qeydiyyatı üçün sorğu gəldi: {Email}", register.Email);

            var result = await _accountService.Register(register);
            if (!result)
            {
                _logger.LogWarning("Qeydiyyat uğursuz oldu: Email artıq istifadə olunub - {Email}", register.Email);
                return BadRequest("Email artıq istifadə olunub.");
            }

            _logger.LogInformation("Qeydiyyat uğurla tamamlandı: {Email}", register.Email);
            return Ok("Qeydiyyat uğurla tamamlandı.");
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail([FromBody] string[] codes)
        {
            _logger.LogInformation("Email təsdiqləmə cəhdi başlandı.");

            var result = await _accountService.VerifyEmailAsync(codes);
            if (!result)
            {
                _logger.LogWarning("Email təsdiqləmə uğursuz oldu. Yanlış və ya vaxtı keçmiş kod.");
                return BadRequest("Kod yanlışdır və ya vaxtı bitmişdir.");
            }

            _logger.LogInformation("Email təsdiqləndi.");
            return Ok("Email təsdiqləndi.");
        }

        [HttpPost]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            _logger.LogInformation("RefreshToken sorğusu gəldi: {Token}", refreshToken);

            var tokenEntity = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken && t.ExpiryDate > DateTime.UtcNow);

            if (tokenEntity == null)
            {
                _logger.LogWarning("Yanlış və ya vaxtı keçmiş refresh token: {Token}", refreshToken);
                return Unauthorized("Invalid or expired refresh token");
            }

            _context.RefreshTokens.Remove(tokenEntity);
            await _context.SaveChangesAsync();

            var tokens = _accountService.GenerateTokens(tokenEntity.UserEmail);

            _logger.LogInformation("Yeni tokenlər yaradıldı: {Email}", tokenEntity.UserEmail);
            return Ok(tokens);
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            _logger.LogInformation("Login sorğusu alındı: {Email}", login.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState yanlışdır: {Errors}", ModelState);
                return BadRequest(ModelState);
            }

            var tokenResponse = await _accountService.Login(login);

            if (tokenResponse == null)
            {
                _logger.LogWarning("Login uğursuz oldu: {Email}", login.Email);
                return Unauthorized("Email və ya şifrə yanlışdır.");
            }

            _logger.LogInformation("Login uğurlu oldu: {Email}", login.Email);
            return Ok(tokenResponse);
        }
    }
}
