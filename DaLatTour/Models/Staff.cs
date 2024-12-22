using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaLatTour.Models
{
    public class Staff
    {
        public int staff_id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string role { get; set; }
        public string password { get; set; }
        public DateTime created_at { get; set; }
    }
}