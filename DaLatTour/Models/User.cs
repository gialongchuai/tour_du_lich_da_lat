using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaLatTour.Models
{
    public class User
    {
        public int customer_id { get; set; }          // Mã khách hàng (khóa chính)
        public string name { get; set; }              // Tên khách hàng
        public string email { get; set; }             // Email (duy nhất)
        public string phone { get; set; }             // Số điện thoại
        public string username { get; set; }          // Tài khoản
        public string password { get; set; }          // Mật khẩu (mã hóa)
        public string address { get; set; }           // Địa chỉ (có thể để trống)
        public DateTime? dob { get ; set; }            // Ngày sinh (có thể để trống)
        public DateTime created_at { get; set; }      // Ngày tạo tài khoản (mặc định hiện tại)
    }
}