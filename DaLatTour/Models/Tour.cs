using System;


namespace DaLatTour.Models
{
    public class Tour
    {
        public int tour_id { get; set; }
        public string tour_name { get; set; }
        public string description { get; set; }
        public string tour_image { get; set; }
        public decimal price { get; set; }
        public int duration { get; set; }
        public string travelby { get; set; }
        public int available_slots { get; set; }
        public DateTime created_at { get; set; } // Thêm trường CreatedAt nếu cần
    }

}