using DomainLayer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Models
{
    public class Reservation :  BaseEntity
    {
        public int UserId { get; set; }
        public string BarberFullName { get; set; }
        public DateTime ReservationTime { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }

    }
}
