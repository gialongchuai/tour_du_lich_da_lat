using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DaLatTour.Models
{
    public class BookingWishlistModel
    {
        [Key]
        public int BookingId { get; set; }  // ID của booking

        [Required]
        public int WishlistId { get; set; }  // ID của Wishlist
        [Required]
        public int? TourId { get; set; }

        [Required]
        public int CustomerId { get; set; }  // ID của khách hàng

        [Required]
        [Display(Name = "Tên khách hàng")]
        public string CustomerName { get; set; }  // Tên của khách hàng

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }  // Email của khách hàng

        [Required]
        [Phone]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }  // Số điện thoại của khách hàng

        [Required]
        [Display(Name = "Ngày khởi hành")]
        [DataType(DataType.Date)]
        public DateTime? DepartureDate { get; set; }  // Ngày khởi hành
        [Required]
        [Display(Name = "Ngày về")]
        [DataType(DataType.Date)]
        public DateTime? ReturnDate { get; set; }

        [Required]
        [Display(Name = "Số người tham gia")]
        public int NumPeople { get; set; }  // Số người tham gia

        [Required]
        [Display(Name = "Tổng giá")]
        [DataType(DataType.Currency)]
        public decimal TotalPrice { get; set; }  // Tổng giá của tour

        [Display(Name = "Yêu cầu đặc biệt")]
        public string SpecialRequests { get; set; }  // Các yêu cầu đặc biệt của khách hàng

        [Required]
        [Display(Name = "Trạng thái đặt tour")]
        public string BookingStatus { get; set; }  // Trạng thái của booking (Pending, Confirmed, etc.)

        [Required]
        [Display(Name = "Ngày đặt")]
        [DataType(DataType.DateTime)]
        public DateTime? BookingDate { get; set; }  // Ngày đặt tour

        [Display(Name = "Tên tour")]
        public string TourName { get; set; }  // Tên của tour
        public string PaymentMethod { get; set; }

        public BookingWishlistModel()
        {
            // Thiết lập giá trị mặc định cho các thuộc tính cần thiết
            BookingStatus = "Pending";  // Đặt mặc định trạng thái là Pending
            BookingDate = DateTime.Now;  // Đặt ngày đặt tour là ngày hiện tại
        }
    }
    public partial class Booking
    {
        private int? _wishlist_id;

        public int? wishlist_id
        {
            get { return this._wishlist_id; }
            set { this._wishlist_id = value; }
        }
    }

}