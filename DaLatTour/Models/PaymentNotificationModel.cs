using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaLatTour.Models
{
    public class PaymentNotificationModel
    {
        public int BookingId { get; set; } 
        public decimal Amount { get; set; } 
        public string Status { get; set; } 
    }
}