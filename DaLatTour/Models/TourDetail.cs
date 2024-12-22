using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaLatTour.Models
{
    public class TourDetail
    {
        public int tour_detail_id { get; set; }  // Mã chi tiết tour (khóa chính)
        public int tour_id { get; set; }         // Mã tour (khóa ngoại)
        public string image_url { get; set; }    // Đường dẫn hình ảnh
        public int num_people { get; set; }      // Số người đi
        public DateTime departure_date { get; set; }  // Ngày đi
        public DateTime return_date { get; set; }     // Ngày về
        public string daily_activities { get; set; }  // Mô tả các ngày

        // Điều hướng khóa ngoại đến model Tour
        public virtual Tour Tour { get; set; }
    }
}