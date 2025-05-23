using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.DTOs.Reservation;
using ServiceLayer.Services.Interfaces;

namespace CutifyApi.Controllers
{
    public class ReservationController : AppController
    {
        private readonly IReservationService _reservationService;
        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }
        [HttpPost]
        public async Task<IActionResult> AddReservation([FromBody] ReservationCreateDto reservationCreate)
        {
            var fullNameCookie = Request.Cookies["FullName"];
            var phoneCookie = Request.Cookies["PhoneNumber"];

            var success = await _reservationService.AddReservationAsync(reservationCreate, fullNameCookie, phoneCookie, Response);
            if (success)
                return Ok(new { message = "Rezervasiya əlavə olundu." });

            return BadRequest(new { message = "Xəta baş verdi." });
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyReservations()
        {

            var reservations = await _reservationService.MyReservationsAsync();
            if (reservations == null || reservations.Count == 0)
                return NotFound("Rezervasiya tapılmadı");

            return Ok(reservations);

        }




    }
}
