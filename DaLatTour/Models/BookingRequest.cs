using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaLatTour.Models
{
    public class BookingRequest
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int NumPeople { get; set; }
        public string PaymentMethod { get; set; }
        public string BookingStatus { get; set; }
    }
}