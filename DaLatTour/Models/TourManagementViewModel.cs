using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaLatTour.Models
{
    public class TourManagementViewModel
    {
        public IEnumerable<BookingModel> Bookings { get; set; }
        public IEnumerable<ReviewModel> CompletedTours { get; set; }
    }

}