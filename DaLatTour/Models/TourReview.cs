using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaLatTour.Models
{
    public class TourReview
    {
        public int review_id { get; set; }         // Mã đánh giá (khóa chính)
        public int customer_id { get; set; }       // Mã khách hàng (khóa ngoại)
        public int tour_id { get; set; }           // Mã tour (khóa ngoại)
        public string review_text { get; set; }    // Nội dung đánh giá
        public int rating { get; set; }            // Điểm đánh giá (1-5)
        public DateTime review_date { get; set; }  // Ngày đánh giá (mặc định là hiện tại)

        // Optional: Navigation properties to link with User and Tour
        public User Customer { get; set; }         // Khách hàng đã đánh giá
        public Tour Tour { get; set; }             // Tour đã được đánh giá
    }
}