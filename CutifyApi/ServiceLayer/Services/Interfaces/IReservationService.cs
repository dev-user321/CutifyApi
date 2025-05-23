using DomainLayer.Models;
using Microsoft.AspNetCore.Http;
using ServiceLayer.DTOs.Reservation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services.Interfaces
{
    public interface IReservationService
    {
        Task<bool> AddReservationAsync(ReservationCreateDto reservationCreate, string fullNameCookie, string phoneCookie, HttpResponse response);
        Task<List<Reservation>> MyReservationsAsync();
    }
}
