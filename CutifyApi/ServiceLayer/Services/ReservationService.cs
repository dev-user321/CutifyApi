using DomainLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Data;
using ServiceLayer.DTOs.Reservation;
using ServiceLayer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services
{
    public class ReservationService : IReservationService
    {
        private readonly AppDbContext _context;
        public ReservationService(AppDbContext context)
        {
            _context = context; 
        }
        public async Task<bool> AddReservationAsync(ReservationCreateDto reservationCreate, string fullNameCookie, string phoneCookie, HttpResponse response)
        {
            if (reservationCreate == null)
                throw new ArgumentNullException(nameof(reservationCreate));

            if (string.IsNullOrWhiteSpace(reservationCreate.FullName) || reservationCreate.FullName == "string")
                reservationCreate.FullName = fullNameCookie;

            if (string.IsNullOrWhiteSpace(reservationCreate.PhoneNumber) || reservationCreate.PhoneNumber == "string")
                reservationCreate.PhoneNumber = phoneCookie;

            var user = await _context.Users
                .Where(m => !m.SoftDelete && m.EmailConfirmed == true)
                .FirstOrDefaultAsync(m => m.Id == reservationCreate.BarberId);

            if (user == null)
                return false;

            var reservation = new Reservation()
            {
                FullName = reservationCreate.FullName,
                BarberFullName = user.LastName + " " + user.Name,
                UserId = reservationCreate.BarberId,
                ReservationTime = reservationCreate.ReservationTime,
                PhoneNumber = reservationCreate.PhoneNumber
            };

            await _context.Reservations.AddAsync(reservation);
            var result = await _context.SaveChangesAsync();

            response.Cookies.Append("FullName", reservationCreate.FullName ?? "", new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = false
            });

            response.Cookies.Append("PhoneNumber", reservationCreate.PhoneNumber ?? "", new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = false
            });

            return result > 0;
        }

        private string GetEmailFromToken(HttpContext httpContext)
        {
            if (httpContext.User.Identity is ClaimsIdentity identity)
            {
                var emailClaim = identity.FindFirst(ClaimTypes.Email);
                return emailClaim?.Value;
            }
            return null;
        }

        public async Task<List<Reservation>> MyReservationsAsync(DateTime dayStart, DateTime dayEnd, HttpContext httpContext)
        {
            var email = GetEmailFromToken(httpContext);
            if (email == null)
                return null;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && !u.SoftDelete && u.EmailConfirmed == true);

            if (user == null)
                return null;

            var reservations = await _context.Reservations
                .Where(r => r.UserId == user.Id &&
                            r.ReservationTime >= dayStart &&
                            r.ReservationTime <= dayEnd)
                .ToListAsync();

            return reservations;
        }

    }
}
