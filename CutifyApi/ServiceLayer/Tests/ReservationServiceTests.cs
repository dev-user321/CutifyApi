using DomainLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using RepositoryLayer.Data;
using ServiceLayer.DTOs.Reservation;
using ServiceLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ServiceLayer.Tests
{
    public class ReservationServiceTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + System.Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task AddReservationAsync_WithValidInput_ShouldAddReservation()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var barber = new AppUser
            {
                Id = 1,
                Name = "John",
                LastName = "Doe",
                EmailConfirmed = true,
                SoftDelete = false
            };
            dbContext.Users.Add(barber);
            await dbContext.SaveChangesAsync();

            var reservationDto = new ReservationCreateDto
            {
                FullName = "Test User",
                PhoneNumber = "123456789",
                ReservationTime = DateTime.Now,
                BarberId = barber.Id
            };

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var mockResponse = new Mock<HttpResponse>();
            var mockCookies = new Mock<IResponseCookies>();
            mockResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);

            var service = new ReservationService(dbContext, mockHttpContextAccessor.Object);

            // Act
            var result = await service.AddReservationAsync(reservationDto, "", "", mockResponse.Object);

            // Assert
            Assert.True(result);
            Assert.Single(dbContext.Reservations);
        }

        [Fact]
        public async Task AddReservationAsync_WithInvalidBarber_ShouldReturnFalse()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();

            var reservationDto = new ReservationCreateDto
            {
                FullName = "Test User",
                PhoneNumber = "123456789",
                ReservationTime = DateTime.Now,
                BarberId = 999 // nonexistent barber
            };

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var mockResponse = new Mock<HttpResponse>();
            var mockCookies = new Mock<IResponseCookies>();
            mockResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);

            var service = new ReservationService(dbContext, mockHttpContextAccessor.Object);

            // Act
            var result = await service.AddReservationAsync(reservationDto, "", "", mockResponse.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task MyReservationsAsync_ShouldReturnUserReservations()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var email = "user@example.com";
            var user = new AppUser
            {
                Id = 1,
                Email = email,
                EmailConfirmed = true,
                SoftDelete = false
            };
            dbContext.Users.Add(user);
            dbContext.Reservations.Add(new Reservation
            {
                UserId = user.Id,
                FullName = "Res One",
                ReservationTime = DateTime.Now,
                PhoneNumber = "123",
                BarberFullName = user.Name + " " + user.LastName
            });
            await dbContext.SaveChangesAsync();

            // Fake ClaimsPrincipal with email
            var claims = new List<Claim> { new Claim(ClaimTypes.Email, email) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            var mockHttpContext = new DefaultHttpContext { User = principal };
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(mockHttpContext);

            var service = new ReservationService(dbContext, mockHttpContextAccessor.Object);

            // Act
            var result = await service.MyReservationsAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Res One", result[0].FullName);
        }

        [Fact]
        public async Task MyReservationsAsync_WithNoUserInToken_ShouldReturnNull()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());

            var service = new ReservationService(dbContext, mockHttpContextAccessor.Object);

            // Act
            var result = await service.MyReservationsAsync();

            // Assert
            Assert.Null(result);
        }
    }

}
