using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaLatTour.Models
{
    public class TourComboModel
    {
        public string combo_id { get; set; }
        public string combo_name { get; set; }       // Tên combo tour
        public string description { get; set; }       // Mô tả combo tour
        public decimal price { get; set; }            // Giá combo tour
        public int hotel_id { get; set; }             // ID khách sạn
        public string hotel_name { get; set; }        // Tên khách sạn
        public int restaurant_id { get; set; }        // ID nhà hàng
        public string restaurant_name { get; set; }   // Tên nhà hàng
        public string img_url { get; set; }           // Đường dẫn ảnh
        public int available_slots { get; set; }           // Đường dẫn ảnh
        public List<ServiceModel> services { get; set; } // Danh sách dịch vụ bổ sung
        public DateTime created_at { get; set; }      // Ngày tạo
        public List<string> additional_imgs { get; set; } // Danh sách ảnh phụ
        public int RemainingSlots { get; set; }
    }


}