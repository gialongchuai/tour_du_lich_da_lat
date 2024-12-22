using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaLatTour.Models
{
    public class BookingComboModel
    {
        public string TourName;
        public int? TourId { get; set; }             // Nullable nếu không có tour thông thường
        public int ComboId { get; set; }             // Combo tour ID
        public int CustomerId { get; set; }          // ID của khách hàng
        public int NumPeople { get; set; }           // Số người tham gia
        public decimal TotalPrice { get; set; }      // Tổng giá tiền
        public string SpecialRequests { get; set; }  // Yêu cầu đặc biệt
        public int? StaffId { get; set; }            // Nullable nếu không có nhân viên phụ trách
        public string CustomerName { get; set; }     // Tên khách hàng
        public string PhoneNumber { get; set; }      // Số điện thoại khách hàng
        public string Email { get; set; }            // Email khách hàng
        public string Address { get; set; }          // Địa chỉ khách hàng
        public string PaymentMethod { get; set; }    // Phương Thức thanh toán

        public DateTime? DepartureDate { get; set; } // Ngày khởi hành (nullable nếu chưa xác định)
    }
}