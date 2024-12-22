using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaLatTour.Models
{
    public class DepartureDateModel
    {
        public DateTime DepartureDate { get; set; }
        public decimal Price { get; set; }
        public int RemainingSlots { get; set; }
    }

}