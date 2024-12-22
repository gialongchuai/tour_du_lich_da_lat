using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaLatTour.Models
{
    public class TourStatisticViewModel
    {
        public string Name { get; set; } // Tên Tour hoặc Combo
        public int TotalBookings { get; set; } // Tổng số lượt đặt
        public int TotalPeople { get; set; } // Tổng số khách
        public decimal TotalRevenue { get; set; } // Tổng doanh thu
    }
}