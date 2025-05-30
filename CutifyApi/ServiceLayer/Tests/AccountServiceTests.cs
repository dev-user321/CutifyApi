using DomainLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Data;
using ServiceLayer.DTOs.Account;
using ServiceLayer.DTOs.Token;
using ServiceLayer.Helper;
using ServiceLayer.Services.Interfaces;
using ServiceLayer.Services;
using Microsoft.EntityFrameworkCore.InMemory;
using Xunit;
using Moq;

namespace ServiceLayer.Tests
{
    public class AccountServiceTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + System.Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var email = "test@example.com";
            var password = "123456";
            var hashedPassword = PasswordHash.HashPassword(password);

            dbContext.Users.Add(new AppUser
            {
                Email = email,
                Password = hashedPassword,
                EmailConfirmed = true,
                SoftDelete = false
            });
            dbContext.SaveChanges();

            var mockEmailService = new Mock<IEmailService>();
            var mockFileService = new Mock<IFileService>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var accountService = new AccountService(dbContext, mockEmailService.Object, mockFileService.Object, mockHttpContextAccessor.Object);

            var loginDto = new LoginDto { Email = email, Password = password };

            // Act
            var result = await accountService.Login(loginDto);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TokenResponseDto>(result);
            Assert.False(string.IsNullOrEmpty(result.AccessToken));
            Assert.False(string.IsNullOrEmpty(result.RefreshToken));
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsNull()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var email = "test@example.com";
            var wrongPassword = "wrong";
            var correctPassword = PasswordHash.HashPassword("correct");

            dbContext.Users.Add(new AppUser
            {
                Email = email,
                Password = correctPassword,
                EmailConfirmed = true,
                SoftDelete = false
            });
            dbContext.SaveChanges();

            var mockEmailService = new Mock<IEmailService>();
            var mockFileService = new Mock<IFileService>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var accountService = new AccountService(dbContext, mockEmailService.Object, mockFileService.Object, mockHttpContextAccessor.Object);

            var loginDto = new LoginDto { Email = email, Password = wrongPassword };

            // Act
            var result = await accountService.Login(loginDto);

            // Assert
            Assert.Null(result);
        }
    }
}
