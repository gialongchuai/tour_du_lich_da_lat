using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaLatTour.Models
{
    public class ServiceModel
    {
        public int service_id { get; set; }          // ID dịch vụ
        public string service_name { get; set; }
        public double price { get; set; }
        public string description { get; set; }
}
}