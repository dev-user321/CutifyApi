using DomainLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions; 
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RepositoryLayer.Data;
using ServiceLayer.DTOs.Account;
using ServiceLayer.Helper;
using ServiceLayer.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ServiceLayer.Services
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IFileService _fileService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AccountService(AppDbContext context, IEmailService emailService, IFileService fileService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _emailService = emailService;
            _fileService = fileService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> Login(LoginDto login)
        {
            var user = await _context.Users.Where(m=>!m.SoftDelete && m.EmailConfirmed == true).FirstOrDefaultAsync(m=>m.Email ==  login.Email);
            bool checkPassword = PasswordHash.VerifyHashedPassword(user.Password,login.Password);
            if(checkPassword)
            {
                var token = GenerateJwtToken(login.Email);
                Console.WriteLine("Token: " + token); 
                return true;
            }
           
            return false;
        }
        public async Task<AppUser> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .Where(u => !u.SoftDelete && u.EmailConfirmed == true)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public string GenerateJwtToken(string email)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("I_dont_know_what_happen_in_here777"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "https://localhost:7003/",
                audience: "https://localhost:7003/",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<bool> Register(RegisterDto register)
        {
            if (register == null) throw new ArgumentNullException(nameof(register));

            var user = await _context.Users.Where(u => !u.SoftDelete)
                                           .FirstOrDefaultAsync(u => u.Email == register.Email);

            if (user != null)
            {
                return false; 
            }

            var hashedPassword = PasswordHash.HashPassword(register.Password);
            var newUser = new AppUser()
            {
                UserImage = register.Image,
                LastName = register.LastName,
                Name = register.Name,
                Email = register.Email,
                Password = hashedPassword,
                Address = register.Address
            };


            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            var code = new Random().Next(1000, 9999).ToString();
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Session.SetString("email_verification_code", code);
                httpContext.Session.SetString("email_to_verify", register.Email);
                httpContext.Session.SetString("method", "register");
            }

            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Email Verification</title>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
    <div style='max-width: 600px; margin: auto; background-color: #fff; padding: 30px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
        <h2 style='color: #333;'>Email Verification</h2>
        <p>Dear user,</p>
        <p>Thank you for registering. Please use the following code to verify your email address:</p>
        <div style='text-align: center; margin: 30px 0;'>
            <span style='display: inline-block; font-size: 32px; font-weight: bold; background-color: #eee; padding: 10px 20px; border-radius: 6px;'>
                {code}
            </span>
        </div>
        <p>This code will expire shortly. If you didn’t request this, please ignore this email.</p>
        <p style='margin-top: 40px;'>Best regards,<br><strong>Sizin Komanda</strong></p>
    </div>
</body>
</html>";

            body = body.Replace("{{code}}", code);
            _emailService.Send(register.Email, "Email Verification Code", body);



            return true;
        }
        public async Task<bool> VerifyEmailAsync(string[] codes)
        {
            string enteredCode = string.Concat(codes);
            string storedCode = _httpContextAccessor.HttpContext.Session.GetString("email_verification_code");
            string method = _httpContextAccessor.HttpContext.Session.GetString("method");

            if (storedCode == null || enteredCode != storedCode)
            {
                return (false);
            }

            string email = _httpContextAccessor.HttpContext.Session.GetString("email_to_verify");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (method == "register" && user != null)
            {
                user.EmailConfirmed = true;
                await _context.SaveChangesAsync();
                ClearVerificationSession(_httpContextAccessor.HttpContext);
                return (true);
            }
            else if (method == "changePassword")
            {

                return (true);
            }

            ClearVerificationSession(_httpContextAccessor.HttpContext);
            return (true);
        }
        private void ClearVerificationSession(HttpContext httpContext)
        {
            httpContext.Session.Remove("email_verification_code");
            httpContext.Session.Remove("email_to_verify");
            httpContext.Session.Remove("method");
        }
    }
}
