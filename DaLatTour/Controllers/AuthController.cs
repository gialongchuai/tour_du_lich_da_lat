using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DaLatTour.Models;

namespace DaLatTour.Controllers
{
    public class AuthController : Controller
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        TourDBDataContext db = new TourDBDataContext(connectionString);
        // GET: Auth
        // GET: Auth - Trang đăng nhập
        public ActionResult AdminLogin()
        {
            // Kiểm tra nếu người dùng đã đăng nhập, điều hướng đến trang phù hợp
            if (Session["UserRole"] != null)
            {
                return RedirectToActionBasedOnRole(Session["UserRole"].ToString());
            }
            return View();
        }

        [HttpPost]
        public ActionResult AdminLogin(string email, string password)
        {
            var staffMember = db.Staffs.FirstOrDefault(s => s.email == email && s.password == password);

            if (staffMember != null)
            {
                // Lưu thông tin vào session
                Session["AdminEmail"] = staffMember.email;
                Session["AdminRole"] = staffMember.role;
                Session["AdminName"] = staffMember.name;

                // Chuyển hướng dựa trên vai trò
                return RedirectToAction("Admin", "Admin");
            }

            // Đăng nhập không thành công, hiển thị thông báo lỗi
            ViewBag.Message = "Email hoặc mật khẩu không chính xác.";
            return View();
        }

        // Đăng xuất
        public ActionResult AdminLogout()
        {
            // Xóa tất cả các session hiện có và điều hướng đến trang đăng nhập
            Session.Clear();
            return RedirectToAction("AdminLogin");
        }

        // Trang quản lý Tours dành cho Admin và Tour_Manager
        [Authorize(Roles = "Admin, Tour_Manager")]
        public ActionResult ManageTours()
        {
            var tours = db.Tours.ToList();
            ViewBag.UserRole = Session["AdminRole"]?.ToString();
            return View(tours);
        }

        // Trang quản lý Booking dành cho Staff
        [Authorize(Roles = "Admin, Staff")]
        public ActionResult ManageBookings()
        {
            var bookings = db.Bookings.ToList();
            ViewBag.UserRole = Session["AdminRole"]?.ToString();
            return View(bookings);
        }

        // Hàm phụ trợ điều hướng dựa trên vai trò người dùng
        private ActionResult RedirectToActionBasedOnRole(string role)
        {
            // Mọi người dùng đều được chuyển hướng đến Admin/Admin
            return RedirectToAction("Admin", "Admin");
        }


        private string HashPassword(string password)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant(); // Chuyển đổi thành chuỗi hex
            }
        }

        public ActionResult Register(string name, string email, string phone, string username, string password)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return Json(new { success = false, message = "Tất cả các trường đều phải được điền." });
            }

            // Băm mật khẩu
            string hashedPassword = HashPassword(password);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO Customer (name, email, phone, username, password) VALUES (@name, @email, @phone, @username, @password)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@phone", phone);
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", hashedPassword);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Đăng ký thành công!" });
            }
            catch (SqlException ex) when (ex.Number == 2627) // Lỗi trùng khóa
            {
                return Json(new { success = false, message = "Tên đăng nhập hoặc email đã tồn tại." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
        public ActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Tên đăng nhập và mật khẩu không được để trống.";
                return View("Index"); // Hoặc trang mà bạn muốn hiển thị
            }

            // Băm mật khẩu người dùng nhập vào
            string hashedPassword = HashPassword(password);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT password, name, email, customer_id FROM Customer WHERE username = @username";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Lấy mật khẩu đã băm từ cơ sở dữ liệu
                                string storedPassword = reader["password"].ToString();

                                // So sánh mật khẩu đã băm
                                if (hashedPassword == storedPassword)
                                {
                                    // Lưu thông tin người dùng vào session
                                    HttpContext.Session["UserName"] = reader["name"].ToString();
                                    HttpContext.Session["UserEmail"] = reader["email"].ToString();
                                    HttpContext.Session["Customer_Id"] = reader["customer_id"].ToString();
                                    HttpContext.Session["User"] = username;
                                    return Json(new { success = true, message = "Đăng nhập thành công.", redirectUrl = Url.Action("TopRated", "Tour") });
                                }
                                else
                                {
                                    return Json(new { success = false, message = "Tên đăng nhập hoặc mật khẩu không đúng, vui lòng thử lại." });
                                }
                            }
                            else
                            {
                                return Json(new { success = false, message = "Tên đăng nhập không tồn tại." });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi đăng nhập: " + ex.Message });
            }
        }
        public ActionResult LoginSuccess()
        {
            return View();
        }


        public ActionResult Logout()
        {
            Session.Clear(); // Xóa tất cả session
            return RedirectToAction("Index", "Tour"); // Chuyển hướng về trang chủ hoặc trang mong muốn
        }

        public ActionResult Dashboard()
        {
            // Chỉ cho phép người dùng đã đăng nhập
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Tour"); 
            }

            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            // Kiểm tra tính hợp lệ của model
            if (ModelState.IsValid)
            {
                string username = Session["User"]?.ToString();

                // Kiểm tra xem người dùng có đăng nhập chưa
                if (username == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để thay đổi mật khẩu." });
                }

                string hashedOldPassword = HashPassword(model.OldPassword);  // Mã hóa mật khẩu cũ
                string hashedNewPassword = HashPassword(model.NewPassword); // Mã hóa mật khẩu mới

                try
                {
                        var customer = db.Customers.Where(c => c.username == username && c.password == hashedOldPassword).FirstOrDefault();

                        if (customer == null)
                        {
                            return Json(new { success = false, message = "Tên đăng nhập không tồn tại." });
                        }

                        // Kiểm tra mật khẩu cũ
                        if (customer!=null)
                        {
                            // Cập nhật mật khẩu mới
                            customer.password = hashedNewPassword;

                            // Lưu thay đổi vào cơ sở dữ liệu
                            db.SubmitChanges();

                            // Trả về kết quả thành công
                            return Json(new { success = true, message = "Đổi mật khẩu thành công!" });
                        }
                        else
                        {
                            // Nếu mật khẩu cũ không đúng
                            return Json(new { success = false, message = "Mật khẩu cũ không đúng." });
                        }
                    }
                catch (Exception ex)
                {
                    // Xử lý lỗi khi thực hiện thao tác với cơ sở dữ liệu
                    return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
                }
            }

            // Trả về thông báo lỗi nếu model không hợp lệ
            return Json(new { success = false, message = "Thông tin nhập vào không hợp lệ." });
        }

        public ActionResult Index()
        {
            return View();
        }

        private string GenerateRandomPassword(int length = 8)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_-";
            StringBuilder password = new StringBuilder();
            Random rand = new Random();

            for (int i = 0; i < length; i++)
            {
                password.Append(validChars[rand.Next(validChars.Length)]);
            }

            return password.ToString();
        }

        private void SendNewPasswordEmailAndUpdateDatabase(string toEmail)
        {
            try
            {
                // Tạo mật khẩu ngẫu nhiên mới
                string newPassword = GenerateRandomPassword(12); // Mật khẩu 12 ký tự

                // Băm mật khẩu mới để bảo mật
                string hashedPassword = HashPassword(newPassword);

                // Cập nhật mật khẩu mới trong cơ sở dữ liệu
                var customer = db.Customers.FirstOrDefault(c => c.email == toEmail);
                if (customer != null)
                {
                    customer.password = hashedPassword;  // Lưu mật khẩu mới vào cơ sở dữ liệu
                    db.SubmitChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                }

                // Gửi email với mật khẩu mới
                SendNewPasswordEmail(toEmail, newPassword);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi xử lý mật khẩu mới: " + ex.Message);
            }
        }

        private void SendNewPasswordEmail(string toEmail, string newPassword)
        {
            try
            {
                // Đường dẫn đến mẫu email trong Views
                var templatePath = Server.MapPath("~/Views/Email/ForgotPassword.cshtml");
                var emailTemplate = System.IO.File.ReadAllText(templatePath);

                // Thay thế các placeholder trong mẫu email với giá trị thực tế
                var body = emailTemplate
                    .Replace("{NewPassword}", newPassword)
                    .Replace("{SupportName}", "DaLatTour Support");

                var subject = "Mật khẩu mới của bạn";

                // Thực hiện gửi email
                using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtpClient.EnableSsl = true;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential("discoverydalat9@gmail.com", "tcrc brwp pqjc ibsq");

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("discoverydalat9@gmail.com", "DaLatTour Support"),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(toEmail);

                    smtpClient.Send(mailMessage);
                    Console.WriteLine("Email gửi thành công đến " + toEmail);
                }
            }
            catch (SmtpException smtpEx)
            {
                Console.WriteLine("Lỗi SMTP: " + smtpEx.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi gửi email: " + ex.Message);
            }
        }

        public ActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "Email không thể để trống." });
            }

            try
            {
                // Kiểm tra xem email có tồn tại trong hệ thống không
                var customer = db.Customers.FirstOrDefault(c => c.email == email);

                if (customer == null)
                {
                    return Json(new { success = false, message = "Email không tồn tại trong hệ thống." });
                }

                // Gửi mật khẩu mới qua email và cập nhật cơ sở dữ liệu
                SendNewPasswordEmailAndUpdateDatabase(email);

                // Trả về JSON cho frontend
                return Json(new { success = true, message = "Mật khẩu mới đã được gửi qua email." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi gửi email: " + ex.Message });
            }
        }


    }
}