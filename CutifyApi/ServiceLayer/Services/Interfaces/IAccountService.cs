using DomainLayer.Models;
using ServiceLayer.DTOs.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services.Interfaces
{
    public interface IAccountService
    {
        Task<bool> Register(RegisterDto register);
        Task<bool> VerifyEmailAsync(string[] codes);
        Task<bool> Login(LoginDto login);
        Task<AppUser> GetUserByEmailAsync(string email);
        string GenerateJwtToken(string email);

    }
}
