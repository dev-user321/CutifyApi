
using RepositoryLayer.Data;
using ServiceLayer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.DTOs.Reservation
{
    public class ReservationCreateDto 
    {
        [Required]
        public int BarberId { get; set; }
        [Required]
        public string BarberFullName { get; set; }
        [Required]
        public DateTime ReservationTime { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string PhoneNumber { get; set; }

    }

}
