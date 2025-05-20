using DomainLayer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Models
{
    public class AppUser : BaseEntity
    {
        public string? Address { get; set; }
        public string? UserImage { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public bool? EmailConfirmed { get; set; } = false;
        public IEnumerable<Reservation> Reservations { get; set; }
    }
}
