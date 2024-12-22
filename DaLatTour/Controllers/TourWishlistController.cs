using DaLatTour.Models;
using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace DaLatTour.Controllers
{
    public class TourWishlistController : Controller
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        TourDBDataContext db = new TourDBDataContext(connectionString);
        private readonly EmailService _emailService = new EmailService();

        // GET: TourWishlist
        public ActionResult TourWishlist(int page = 1)
        {
            IEnumerable<TourWishlistModel> wishlists = GetTourWishlists().ToList();
            wishlists = wishlists.GroupBy(w => w.WishlistId).Select(g => g.First()).ToList();
            int pageSize = 6;
            int totalWishlists = wishlists.Count();
            var pagedWishlists = wishlists.Skip((page - 1) * pageSize).Take(pageSize);

            ViewBag.TotalWishlists = totalWishlists;
            ViewBag.CurrentPage = page;

            return View(pagedWishlists);
        }

        private IEnumerable<TourWishlistModel> GetTourWishlists()
        {
            List<TourWishlistModel> wishlists = new List<TourWishlistModel>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Truy vấn SQL để lấy thông tin wishlist
                string sql = @"
            SELECT 
                tw.wishlist_id, 
                tw.wishlist_name, 
                tw.hotel_id, 
                tw.img_url, 
                tw.created_at, 
                tw.restaurant_id,
                r.restaurant_name,
                tw.description,
                h.hotel_name,
                tw.img_url,
                s.service_id, 
                s.service_name,
                tw.created_at
            FROM 
                TourWishlist tw
            LEFT JOIN
                Hotel h ON tw.hotel_id = h.hotel_id
            LEFT JOIN
                Restaurant r ON tw.restaurant_id = r.restaurant_id
            LEFT JOIN
                WishlistServices tws ON tw.wishlist_id = tws.wishlist_id 
            LEFT JOIN
                Tour_Services s ON tws.service_id = s.service_id";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        // Tạo từng wishlist và thêm vào danh sách
                        wishlists.Add(new TourWishlistModel
                        {
                            WishlistId = (int)reader["wishlist_id"],
                            WishlistName = reader["wishlist_name"].ToString(),
                            description = reader["description"].ToString(),
                            img_url = reader["img_url"].ToString(),
                            created_at = (DateTime)reader["created_at"],
                            hotel_id = (int)reader["hotel_id"],
                            hotel_name = reader["hotel_name"]?.ToString(),
                            services = new List<ServiceModel>(), // Khởi tạo danh sách dịch vụ
                            additional_imgs = new List<string>() // Khởi tạo danh sách ảnh bổ sung
                        });
                    }
                }
            }
            return wishlists;
        }



        public ActionResult TourWishlistDetail(int wishlistId)
        {
            var wishlist = GetWishlistDetail(wishlistId);
            return View(wishlist);
        }


        private TourWishlistModel GetWishlistDetail(int wishlistId)
        {
            TourWishlistModel wishlist = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Truy vấn để lấy thông tin chi tiết về wishlist
                string sql = @"
        SELECT 
            w.wishlist_id,
            w.wishlist_name, 
            w.description, 
            w.img_url, 
            w.created_at,
            w.hotel_id, 
            h.hotel_name,
            w.price,
            w.restaurant_id,
            r.restaurant_name
        FROM 
            TourWishlist w
        LEFT JOIN 
            Hotel h ON w.hotel_id = h.hotel_id
        LEFT JOIN 
             Restaurant r ON w.restaurant_id = r.restaurant_id
        WHERE 
            w.wishlist_id = @wishlist_id;";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    // Thêm tham số cho truy vấn
                    command.Parameters.AddWithValue("@wishlist_id", wishlistId);
                    connection.Open(); // Mở kết nối
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        // Khởi tạo đối tượng TourWishlistModel với dữ liệu từ cơ sở dữ liệu
                        wishlist = new TourWishlistModel
                        {
                            WishlistId = (int)reader["wishlist_id"],
                            WishlistName = reader["wishlist_name"].ToString(),
                            description = reader["description"].ToString(),
                            img_url = reader["img_url"].ToString(),
                            created_at = (DateTime)reader["created_at"],
                            hotel_id = (int)reader["hotel_id"],
                            hotel_name = reader["hotel_name"]?.ToString(),
                            services = new List<ServiceModel>(), // Khởi tạo danh sách dịch vụ
                            additional_imgs = new List<string>(), // Khởi tạo danh sách ảnh bổ sung
                            price = (decimal)reader["price"],
                            restaurant_id = (int)reader["restaurant_id"],
                            restaurant_name = reader["restaurant_name"]?.ToString()
                        };
                    }
                    reader.Close();
                }

                // Truy vấn để lấy danh sách ảnh bổ sung
                string imgSql = "SELECT img_url FROM ImgList WHERE wishlist_id = @wishlist_id";

                using (SqlCommand imgCommand = new SqlCommand(imgSql, connection))
                {
                    imgCommand.Parameters.AddWithValue("@wishlist_id", wishlistId);
                    SqlDataReader imgReader = imgCommand.ExecuteReader();
                    while (imgReader.Read())
                    {
                        wishlist.additional_imgs.Add(imgReader["img_url"].ToString());
                    }
                    imgReader.Close(); // Đảm bảo đóng SqlDataReader
                }

                // Truy vấn để lấy danh sách dịch vụ bổ sung
                string serviceSql = @"
                SELECT 
                    ts.service_name, 
                    ts.price, 
                    ts.description 
                FROM 
                    WishlistServices ws
                JOIN 
                    Tour_Services ts ON ws.service_id = ts.service_id
                WHERE 
                    ws.wishlist_id = @wishlist_id;";

                using (SqlCommand serviceCommand = new SqlCommand(serviceSql, connection))
                {
                    serviceCommand.Parameters.AddWithValue("@wishlist_id", wishlistId);
                    SqlDataReader serviceReader = serviceCommand.ExecuteReader();
                    while (serviceReader.Read())
                    {
                        // Thêm từng dịch vụ vào danh sách services
                        wishlist.services.Add(new ServiceModel
                        {
                            service_name = serviceReader["service_name"].ToString(),
                            price = serviceReader["price"] != DBNull.Value ? Convert.ToDouble(serviceReader["price"]) : 0,
                            description = serviceReader["description"].ToString()
                        });
                    }
                    serviceReader.Close();
                }
            }

            return wishlist;
        }



        [HttpPost]
        public ActionResult AddToWishlist(int serviceId, int wishlistId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "INSERT INTO WishlistServices (wishlist_id, service_id) VALUES (@wishlist_id, @service_id)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@wishlist_id", wishlistId);
                    command.Parameters.AddWithValue("@service_id", serviceId);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("WishlistDetail", new { wishlistId = wishlistId });
        }

        [HttpPost]
        public ActionResult RemoveFromWishlist(int serviceId, int wishlistId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "DELETE FROM WishlistServices WHERE wishlist_id = @wishlist_id AND service_id = @service_id";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@wishlist_id", wishlistId);
                    command.Parameters.AddWithValue("@service_id", serviceId);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction("WishlistDetail", new { wishlistId = wishlistId });
        }
        [HttpGet]
        public ActionResult CreateBooking(int wishlistId, string departureDate, string returnDate)
        {
            // Kiểm tra giá trị đầu vào
            if (wishlistId <= 0 || string.IsNullOrEmpty(departureDate) || string.IsNullOrEmpty(returnDate))
            {
                return new HttpStatusCodeResult(400, "Dữ liệu không hợp lệ.");
            }

            DateTime departure, returnDateValue;
            if (!DateTime.TryParse(departureDate, out departure) || !DateTime.TryParse(returnDate, out returnDateValue))
            {
                return new HttpStatusCodeResult(400, "Ngày khởi hành hoặc ngày về không hợp lệ.");
            }

            // Lấy thông tin wishlist từ cơ sở dữ liệu
            var wishlist = db.TourWishlists.FirstOrDefault(w => w.wishlist_id == wishlistId);
            if (wishlist == null)
            {
                return HttpNotFound("Không tìm thấy wishlist.");
            }

            // Tạo model booking và chuyển đến view nhập thông tin thanh toán
            var bookingModel = new BookingWishlistModel
            {
                WishlistId = wishlist.wishlist_id,
                DepartureDate = departure
            };

            return View(bookingModel);
        }

        private int SaveBookingToDatabase(BookingWishlistModel model)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                INSERT INTO Booking (customer_id, tour_id, wishlist_id, departure_date, num_people, total_price, special_requests, booking_status, booking_date) 
                OUTPUT INSERTED.booking_id
                VALUES (@CustomerId, @TourId, @wishlistId, @DepartureDate, @NumPeople,@TotalPrice , @SpecialRequests, @BookingStatus, @BookingDate)";

                    var customerId = HttpContext.Session["Customer_Id"];
                    if (customerId == null)
                    {
                        throw new Exception("Session không chứa Customer_Id.");
                    }
                    model.CustomerId = int.Parse(customerId.ToString());

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CustomerId", model.CustomerId);
                        command.Parameters.AddWithValue("@TourId", model.TourId ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@WishlistId", model.WishlistId);
                        command.Parameters.AddWithValue("@DepartureDate", model.DepartureDate ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@NumPeople", model.NumPeople);
                        command.Parameters.AddWithValue("@TotalPrice", 0);
                        command.Parameters.AddWithValue("@SpecialRequests", string.IsNullOrEmpty(model.SpecialRequests) ? (object)DBNull.Value : model.SpecialRequests);
                        command.Parameters.AddWithValue("@BookingStatus", "Pending");
                        command.Parameters.AddWithValue("@BookingDate", DateTime.Now);

                        // Lấy bookingId từ cơ sở dữ liệu
                        int? bookingId = (int?)command.ExecuteScalar();
                        if (bookingId == null)
                        {
                            throw new Exception("Không thể lấy bookingId sau khi lưu thông tin.");
                        }
                        return bookingId.Value;
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                // Log lỗi SQL để debug
                Console.WriteLine("Lỗi SQL: " + sqlEx.Message);
                throw new Exception("Lỗi SQL: " + sqlEx.Message);
            }
            catch (Exception ex)
            {
                // Log lỗi chung để debug
                Console.WriteLine("Lỗi: " + ex.Message);
                throw new Exception("Có lỗi xảy ra khi lưu dữ liệu vào cơ sở dữ liệu: " + ex.Message);
            }
        }



        [HttpPost]
        public ActionResult CreateBooking(BookingWishlistModel model)
        {
           

            // Kiểm tra dữ liệu hợp lệ
            if (ModelState.IsValid)
            {
                // Lưu thông tin đặt chỗ và lấy mã đặt chỗ (bookingId)
                int bookingId = SaveBookingToDatabase(model);

                // Xử lý theo phương thức thanh toán
                if (model.PaymentMethod == "Cash")
                {
                    // Xử lý thanh toán tiền mặt
                    return RedirectToAction("BookingConfirmation", new { bookingId = bookingId });
                }
                else if (model.PaymentMethod == "Bank_Transfer")
                {
                    TempData["PaymentMethod"] = model.PaymentMethod;
                    return RedirectToAction("Payment", new { bookingId = bookingId });
                }
                else if (model.PaymentMethod == "MoMo")
                {
                    return RedirectToAction("MoMoPayment", new { bookingId = bookingId });
                }
            }

            // Trường hợp dữ liệu không hợp lệ
            return View(model);
        }



        [HttpGet]
        public ActionResult Payment(int bookingId)
        {
            // Lấy thông tin đặt chỗ từ database
            var booking = db.Bookings.FirstOrDefault(b => b.booking_id == bookingId);
            if (booking == null)
            {
                return HttpNotFound("Không tìm thấy đơn hàng.");
            }

            // Truyền thông tin booking sang view
            return View(booking);
        }

        [HttpPost]
        public JsonResult ConfirmPayment(int bookingId, string paymentMethod)
        {
            if (paymentMethod == null)
            {
                paymentMethod = TempData["PaymentMethod"] as string;
            }

            if (paymentMethod == "MoMo")
            {
                paymentMethod = "Bank_Transfer";
            }

            try
            {
                // Tìm đặt hàng trong cơ sở dữ liệu
                var booking = db.Bookings.FirstOrDefault(b => b.booking_id == bookingId);
                if (booking == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
                }

                // Xác định trạng thái thanh toán
                string status = paymentMethod == "Cash" ? "Pending" : "Completed";

                // Tạo và lưu hóa đơn
                var invoice = new Invoice
                {
                    booking_id = bookingId,
                    invoice_date = DateTime.Now,
                    total_amount = booking.total_price,
                    payment_method = paymentMethod,
                    payment_status = status
                };

                db.Invoices.InsertOnSubmit(invoice);

                // Cập nhật trạng thái đặt hàng
                booking.booking_status = (paymentMethod == "Cash") ? "Pending" : "Confirmed";
                db.SubmitChanges();

                // Gửi email xác nhận nếu phương thức là chuyển khoản ngân hàng
                if (paymentMethod == "Bank_Transfer")
                {
                    SendConfirmationEmail(booking);
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi trong ConfirmPayment: " + ex.Message);
                return Json(new { success = false, message = "Đã xảy ra lỗi trong quá trình xác nhận thanh toán." });
            }
        }
        public ActionResult BookingConfirmation(int bookingId)
        {
            var booking = db.Bookings.FirstOrDefault(b => b.booking_id == bookingId);
            if (booking == null)
            {
                return HttpNotFound("Không tìm thấy đơn hàng.");
            }

            var model = new BookingWishlistModel
            {
                CustomerName = booking.Customer.name,
                WishlistId = bookingId,
                DepartureDate = booking.departure_date ?? DateTime.Now,
                NumPeople = booking.num_people,
                TotalPrice = booking.total_price,
                Email = booking.Customer.email
            };

            return View(model);
        }



        private void SendConfirmationEmail(Booking booking)
        {
            try
            {
                // Adjusted line for MVC context
                var templatePath = Server.MapPath("~/Views/Email/ConfirmationEmailTemplate.cshtml");
                var emailTemplate = System.IO.File.ReadAllText(templatePath);

                // Replace placeholders with actual values
                var body = emailTemplate
                    .Replace("{CustomerName}", booking.Customer.name)
                    .Replace("{BookingId}", booking.booking_id.ToString())
                    .Replace("{DepartureDate}", booking.departure_date?.ToString("dd/MM/yyyy") ?? "N/A")
                    .Replace("{NumPeople}", booking.num_people.ToString())
                    .Replace("{TotalPrice}", booking.total_price.ToString("C"));

                var customerEmail = booking.Customer.email;
                var subject = "Xác nhận thanh toán thành công";

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
                    mailMessage.To.Add(customerEmail);

                    smtpClient.Send(mailMessage);
                    Console.WriteLine("Email sent successfully to " + customerEmail);
                }
            }
            catch (SmtpException smtpEx)
            {
                Console.WriteLine("SMTP Error: " + smtpEx.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }
        [HttpGet]
        public ActionResult PaymentStatus(int bookingId)
        {
            var booking = db.Bookings.FirstOrDefault(b => b.booking_id == bookingId);
            if (booking == null)
            {
                return HttpNotFound("Không tìm thấy đơn hàng.");
            }

            return View(booking);
        }
        public ActionResult MoMoPayment(int bookingId)
        {
            // Cấu hình giao thức TLS
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

            // Lấy thông tin đặt chỗ từ cơ sở dữ liệu
            var booking = db.Bookings.FirstOrDefault(b => b.booking_id == bookingId);
            if (booking == null) return RedirectToAction("CreateBooking", new { error = "Không tìm thấy đơn hàng." });

            // Cấu hình MoMo
            string endpoint = "https://test-payment.momo.vn/gw_payment/transactionProcessor";
            string partnerCode = "MOMO", accessKey = "F8BBA842ECF85", secretKey = "K951B6PE1waDMi640xX08PD3vg6EkVlz";
            string orderId = $"ORDER{bookingId}-{DateTime.Now.Ticks}", requestId = orderId;
            string amount = booking.total_price.ToString("F0"); // Đảm bảo số tiền là số nguyên
            string returnUrl = Url.Action("MoMoReturn", "TourCombo", null, Request.Url.Scheme);
            string notifyUrl = Url.Action("MoMoNotify", "TourCombo", null, Request.Url.Scheme);
            string rawHash = $"partnerCode={partnerCode}&accessKey={accessKey}&requestId={requestId}&amount={amount}&orderId={orderId}&orderInfo=Thanh toán đơn hàng {bookingId}&returnUrl={returnUrl}&notifyUrl={notifyUrl}&extraData=";
            string signature = HmacSHA256(rawHash, secretKey);

            // Tạo dữ liệu gửi đến MoMo
            var message = new
            {
                partnerCode,
                accessKey,
                requestId,
                amount,
                orderId,
                orderInfo = $"Thanh toán đơn hàng {bookingId}",
                returnUrl,
                notifyUrl,
                extraData = "",
                requestType = "captureMoMoWallet",
                signature
            };

            // Sử dụng HttpClient để gửi yêu cầu POST
            using (var client = new HttpClient())
            {
                var response = client.PostAsync(endpoint, new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json")).Result;
                var responseContent = response.Content.ReadAsStringAsync().Result;
                dynamic momoResponse = JsonConvert.DeserializeObject(responseContent);
                Console.WriteLine($"Phản hồi từ MoMo: {responseContent}");

                if (response.IsSuccessStatusCode && momoResponse?.payUrl != null)
                {
                    return Redirect(momoResponse.payUrl.ToString());
                }
                else
                {
                    string errorMessage = momoResponse?.message ?? "Thanh toán MoMo thất bại.";
                    Console.WriteLine($"Lỗi từ MoMo: {errorMessage}");
                    return RedirectToAction("CreateBooking", new { error = errorMessage });
                }
            }
        }

        private static string HmacSHA256(string text, string key)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                return BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", "").ToLower();
            }
        }
        public ActionResult MoMoReturn(string orderId, string resultCode, string message)
        {
            string errorCode = Request.QueryString["errorCode"]; // Đây là tham số cần kiểm tra thay vì resultCode
            string bookingIdStr = orderId?.Split('-')?[0].Replace("ORDER", "");
            if (int.TryParse(bookingIdStr, out int bookingId))
            {
                var booking = db.Bookings.FirstOrDefault(b => b.booking_id == bookingId);
                if (booking != null)
                {
                    if (errorCode == "0")
                    {
                        // Thanh toán thành công
                        booking.booking_status = "Confirmed";
                        string paymentMethod = "MoMo";
                        ConfirmPayment(bookingId, paymentMethod);
                        db.SubmitChanges();
                        ViewBag.Message = "Thanh toán thành công!";
                        return RedirectToAction("BookingConfirmation", new { bookingId = booking.booking_id });
                    }
                    else
                    {
                        // Thanh toán thất bại
                        booking.booking_status = "Cancelled";
                        db.SubmitChanges();
                        ViewBag.Message = $"Thanh toán không thành công: {message ?? "Lỗi không xác định"}";

                    }
                }
                else
                {
                    ViewBag.Message = "Không tìm thấy đơn hàng.";
                }
            }
            else
            {
                ViewBag.Message = "Mã đơn hàng không hợp lệ.";
            }

            return View(); // Hoặc hiển thị trang phù hợp khi gặp lỗi
        }



        [HttpPost]
        public ActionResult MoMoNotify(string orderId, string resultCode, string message)
        {
            try
            {
                string bookingIdStr = orderId?.Split('-')?[0].Replace("ORDER", "");
                if (int.TryParse(bookingIdStr, out int bookingId))
                {
                    var booking = db.Bookings.FirstOrDefault(b => b.booking_id == bookingId);
                    if (booking != null)
                    {
                        booking.booking_status = resultCode == "0" ? "Confirmed" : "Cancelled";
                        db.SubmitChanges();
                    }
                }
                return Json(new { status = "success" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MoMoNotify: {ex.Message}");
                return Json(new { status = "error", message = "Lỗi trong quá trình xử lý." });
            }
        }

    }
}