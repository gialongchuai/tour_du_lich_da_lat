using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaLatTour.Models
{
    public class Booking
    {
        public int customer_id { get; set; }
        public int tour_id { get; set; } // Đảm bảo tour_id là không nullable
        public DateTime booking_date { get; set; }
        public decimal total_price { get; set; }
        public int num_people { get; set; }
        public string booking_status { get; set; }
        public string special_requests { get; set; }
    }
}