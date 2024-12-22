using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaLatTour.Models
{
    public class BookingModel
    {
        public int BookingId { get; set; }
        public string CustomerName { get; set; }
        public DateTime? BookingDate { get; set; }
        public decimal TotalPrice { get; set; }
        public int NumPeople { get; set; }
        public string BookingStatus { get; set; }
        public DateTime? DepartureDate { get; set; }
        public string TourName { get; set; }

        ///Đánh giá tour
        public int? Combo_id { get; set; }
        public int? IsReview { get; set; }



    }

}