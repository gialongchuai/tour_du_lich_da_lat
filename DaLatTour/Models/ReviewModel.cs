using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaLatTour.Models
{
    public class ReviewModel
    {
        public int ReviewId { get; set; }
        public int TourId { get; set; }
        public string ReviewContent { get; set; }
        public string TourName { get; set; }
        public int Rating { get; set; }
        public int CustomerId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}