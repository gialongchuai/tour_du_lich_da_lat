using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaLatTour.Models
{
    public class ScheduleViewModel
    {

        public int? TourId { get; set; }          // Dùng cho tour thường
        public int? ComboId { get; set; }         // Dùng cho combo tour
        public DateTime DepartureDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public int AvailableSlots { get; set; }
        public decimal? Price { get; set; }       // Chỉ dùng cho combo tour
        public string DailyActivities { get; set; } // Hoạt động hàng ngày
        public string TourType { get; set; }  // thuộc tính TourType

    }

}