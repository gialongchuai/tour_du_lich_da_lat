using DaLatTour.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using System.Transactions;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http.Json;
using System.Security.Authentication;

namespace DaLatTour.Controllers
{
    public class TourComboController : Controller
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        TourDBDataContext db = new TourDBDataContext(connectionString);
        private readonly EmailService _emailService = new EmailService();

        // GET: TourCombo
        public ActionResult TourCombo(int page = 1)
        {
            IEnumerable<TourComboModel> tourCombos = GetTourCombos().ToList(); // Chuyển đổi thành danh sách để lấy tổng số tour
            int pageSize = 6; // Số lượng tour trên mỗi trang
            int totalTours = tourCombos.Count(); // Tổng số tour
            var pagedTours = tourCombos.Skip((page - 1) * pageSize).Take(pageSize); // Phân trang

            ViewBag.TotalTours = totalTours; // Lưu tổng số tour vào ViewBag để sử dụng trong view
            ViewBag.CurrentPage = page; // Lưu trang hiện tại vào ViewBag

            //var tourCombos = GetTourCombos(); 
            return View(pagedTours); // Trả về view với danh sách combo tour
        }
        private IEnumerable<TourComboModel> GetTourCombos()
        {
            List<TourComboModel> tourPackages = new List<TourComboModel>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = @"
            SELECT 
                tc.combo_id,
                tc.combo_name, 
                tc.description, 
                tc.price, 
                tc.hotel_id, 
                h.hotel_name, 
                tc.restaurant_id, 
                r.restaurant_name, 
                tc.img_url, 
                tc.created_at,
                s.service_id, 
                s.service_name 
            FROM 
                TourCombo tc
            LEFT JOIN 
                Hotel h ON tc.hotel_id = h.hotel_id
            LEFT JOIN 
                Restaurant r ON tc.restaurant_id = r.restaurant_id
            LEFT JOIN 
                TourCombo_Services tcs ON tc.combo_id = tcs.combo_id  -- Lấy các dịch vụ liên quan
            LEFT JOIN 
                Tour_Services s ON tcs.service_id = s.service_id"; // Lấy tên dịch vụ

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        // Tìm combo tour đã tồn tại trong danh sách
                        var existingCombo = tourPackages.FirstOrDefault(t => t.combo_name == reader["combo_name"].ToString());
                        if (existingCombo == null)
                        {
                            // Nếu không tồn tại, tạo mới
                            existingCombo = new TourComboModel
                            {
                                combo_id = reader["combo_id"].ToString(),
                                combo_name = reader["combo_name"].ToString(),
                                description = reader["description"].ToString(),
                                price = (decimal)reader["price"],
                                hotel_id = (int)reader["hotel_id"],
                                restaurant_id = (int)reader["restaurant_id"],
                                img_url = reader["img_url"].ToString(),
                                created_at = (DateTime)reader["created_at"],
                                hotel_name = reader["hotel_name"]?.ToString(),
                                restaurant_name = reader["restaurant_name"]?.ToString(),
                                services = new List<ServiceModel>() // Khởi tạo danh sách dịch vụ
                            };
                            tourPackages.Add(existingCombo);
                        }

                        // Thêm dịch vụ vào combo
                        if (reader["service_name"] != DBNull.Value)
                        {
                            existingCombo.services.Add(new ServiceModel
                            {
                                service_id = (int)reader["service_id"],
                                service_name = reader["service_name"].ToString()
                            });
                        }
                    }
                }
            }
            return tourPackages; // Trả về danh sách combo tour
        }
        public ActionResult TourComboDetail(int comboid)
        {
            var tourCombo = GetTourComboDetail(comboid);
            var departureDates = GetDepartureDates(comboid);
            ViewBag.DepartureDates = departureDates;
            tourCombo.RemainingSlots = departureDates.Sum(d => d.RemainingSlots);
            return View(tourCombo);
        }
        private List<DepartureDateModel> GetDepartureDates(int comboId)
        {
            List<DepartureDateModel> departureDates = new List<DepartureDateModel>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = @"
                           SELECT 
                                tcd.departure_date, 
                                tcd.price, 
                                (tcd.available_slots - COALESCE(SUM(b.num_people), 0)) AS remaining_slots
                            FROM 
                                TourComboDeparture tcd
                            LEFT JOIN 
                                Booking b ON tcd.combo_id = b.combo_id 
                                AND tcd.departure_date = b.departure_date
                            WHERE 
                                tcd.combo_id = @combo_id
                            GROUP BY 
                                tcd.departure_date, tcd.price, tcd.available_slots;
                            ";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@combo_id", comboId);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        departureDates.Add(new DepartureDateModel
                        {
                            DepartureDate = (DateTime)reader["departure_date"],
                            Price = (decimal)reader["price"],
                            RemainingSlots = (int)reader["remaining_slots"]
                        });
                    }
                }
            }

            return departureDates;
        }
        private TourComboModel GetTourComboDetail(int comboId)
        {
            TourComboModel tourCombo = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Truy vấn để lấy thông tin chi tiết về combo tour
                string sql = @"
                SELECT 
                    tc.combo_id,
                    tc.combo_name, 
                    tc.description, 
                    tc.price, 
                    tc.hotel_id, 
                    h.hotel_name, 
                    tc.restaurant_id, 
                    r.restaurant_name, 
                    tc.img_url, 
                    tc.created_at
                FROM 
                    TourCombo tc
                LEFT JOIN 
                    Hotel h ON tc.hotel_id = h.hotel_id
                LEFT JOIN 
                    Restaurant r ON tc.restaurant_id = r.restaurant_id
                WHERE 
                    tc.combo_id = @combo_id;";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    // Thêm tham số cho truy vấn
                    command.Parameters.AddWithValue("@combo_id", comboId);
                    connection.Open(); // Mở kết nối
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        // Khởi tạo đối tượng TourComboModel với dữ liệu từ cơ sở dữ liệu
                        tourCombo = new TourComboModel
                        {
                            combo_id = reader["combo_id"].ToString(),
                            combo_name = reader["combo_name"].ToString(),
                            description = reader["description"].ToString(),
                            price = (decimal)reader["price"],
                            hotel_id = (int)reader["hotel_id"],
                            hotel_name = reader["hotel_name"]?.ToString(),
                            restaurant_id = (int)reader["restaurant_id"],
                            restaurant_name = reader["restaurant_name"]?.ToString(),
                            img_url = reader["img_url"].ToString(),
                            created_at = (DateTime)reader["created_at"],
                            additional_imgs = new List<string>(), // Khởi tạo danh sách ảnh phụ
                            services = new List<ServiceModel>() // Khởi tạo danh sách dịch vụ
                        };
                    }
                    reader.Close();
                }

                // Truy vấn để lấy danh sách ảnh phụ
                string imgSql = "SELECT img_url FROM ImgList WHERE combo_id = @combo_id";

                using (SqlCommand imgCommand = new SqlCommand(imgSql, connection))
                {
                    imgCommand.Parameters.AddWithValue("@combo_id", comboId);
                    SqlDataReader imgReader = imgCommand.ExecuteReader();
                    while (imgReader.Read())
                    {
                        tourCombo.additional_imgs.Add(imgReader["img_url"].ToString());
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
                    TourCombo_Services tcs
                JOIN 
                    Tour_Services ts ON tcs.service_id = ts.service_id
                WHERE 
                    tcs.combo_id = @combo_id;";

                using (SqlCommand serviceCommand = new SqlCommand(serviceSql, connection))
                {
                    serviceCommand.Parameters.AddWithValue("@combo_id", comboId);
                    SqlDataReader serviceReader = serviceCommand.ExecuteReader();
                    while (serviceReader.Read())
                    {
                        // Thêm từng dịch vụ vào danh sách services
                        tourCombo.services.Add(new ServiceModel
                        {
                            service_name = serviceReader["service_name"].ToString(),
                            price = serviceReader["price"] != DBNull.Value ? Convert.ToDouble(serviceReader["price"]) : 0,
                            description = serviceReader["description"].ToString()
                        });
                    }
                    serviceReader.Close();
                }
            }

            return tourCombo; // Trả về chi tiết combo tour cùng với danh sách dịch vụ
        }
        [HttpGet]
        public ActionResult CreateBooking(int comboId, string departureDate, string price)
        {
            try
            {

                ViewBag.IsLoggedIn = HttpContext.Session["Customer_Id"] != null;

                if (!DateTime.TryParseExact(departureDate, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                {
                    throw new FormatException("Định dạng ngày khởi hành không hợp lệ.");
                }

                var cleanPrice = price.Replace("$", "").Replace(",", "").Trim();
                if (!decimal.TryParse(cleanPrice, out decimal parsedPrice))
                {
                    throw new FormatException("Giá không hợp lệ.");
                }

                int remainingSlots = GetAvailableSlots(comboId, parsedDate);
                ViewBag.RemainingSlots = remainingSlots;

                var model = new BookingComboModel
                {
                    ComboId = comboId,
                    DepartureDate = parsedDate,
                    TotalPrice = parsedPrice
                };

                return View(model);
            }
            catch (FormatException ex)
            {
                ViewBag.Error = "Lỗi định dạng dữ liệu: " + ex.Message;
                return View(new BookingComboModel());
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Đã xảy ra lỗi trong quá trình xử lý: " + ex.Message;
                return View(new BookingComboModel());
            }
        }
        [HttpPost]
        public ActionResult CreateBooking(BookingComboModel model)
        {
            ViewBag.IsLoggedIn = HttpContext.Session["Customer_Id"] != null;

            if (!ViewBag.IsLoggedIn)
            {
                ModelState.AddModelError("", "Vui lòng đăng nhập trước khi đặt tour.");
                ViewBag.ShowLoginPopup = true;
                ViewBag.DepartureDate = model.DepartureDate;
                return View(model);
            }

            int remainingSlots = GetAvailableSlots(model.ComboId, model.DepartureDate);
            if (model.NumPeople > remainingSlots)
            {
                ViewBag.ShowSlotErrorPopup = true;
                ViewBag.RemainingSlots = remainingSlots;
                return View(model);
            }

            if (ModelState.IsValid)
            {
                // Lưu thông tin đặt chỗ và lấy mã đặt chỗ (bookingId)
                int bookingId = SaveBookingToDatabase(model);

                // Kiểm tra phương thức thanh toán và chuyển hướng phù hợp
                if (model.PaymentMethod == "Cash")
                {
                    // Xử lý cho thanh toán tiền mặt hoặc chuyển hướng xác nhận
                    return RedirectToAction("BookingConfirmation", new { bookingId = bookingId });
                }
                else if (model.PaymentMethod == "Bank_Transfer")
                {
                    TempData["PaymentMethod"] = model.PaymentMethod;
                    // Chuyển hướng đến trang thanh toán khi chọn chuyển khoản
                    return RedirectToAction("Payment", new { bookingId = bookingId });
                }
                else if (model.PaymentMethod == "MoMo")
                {
                    // Chuyển hướng đến phương thức thanh toán MoMo
                    return RedirectToAction("MoMoPayment", new { bookingId = bookingId });
                }
            }

            return View(model);
        }
        public ActionResult BookingConfirmation(int bookingId)
        {
            var booking = db.Bookings.FirstOrDefault(b => b.booking_id == bookingId);
            if (booking == null)
            {
                return HttpNotFound("Không tìm thấy đơn hàng.");
            }

            var model = new BookingComboModel
            {
                CustomerName = booking.Customer.name,
                ComboId = bookingId,
                TourName = booking.TourCombo.combo_name ?? "Tour Not Found",
                DepartureDate = booking.departure_date ?? DateTime.Now,
                NumPeople = booking.num_people,
                TotalPrice = booking.total_price,
                Email = booking.Customer.email
            };

            return View(model);
        }
        [HttpGet]
        public ActionResult Payment(int bookingId)
        {
            var booking = db.Bookings.FirstOrDefault(b => b.booking_id == bookingId);
            if (booking == null)
            {
                return HttpNotFound("Không tìm thấy đơn hàng.");
            }

            return View(booking);
        }
        [HttpPost]
        public JsonResult ConfirmPayment(int bookingId, string paymentMethod)
        {
            if(paymentMethod==null)
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
        private int SaveBookingToDatabase(BookingComboModel model)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                INSERT INTO Booking (customer_id, tour_id, combo_id, departure_date, num_people, total_price, special_requests, booking_status, booking_date) 
                OUTPUT INSERTED.booking_id
                VALUES  (@CustomerId, @TourId, @ComboId, @DepartureDate, @NumPeople, @TotalPrice, @SpecialRequests, @BookingStatus, @BookingDate)";

                    var customerId = HttpContext.Session["Customer_Id"];
                    model.CustomerId = int.Parse(customerId.ToString());

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CustomerId", model.CustomerId);
                        command.Parameters.AddWithValue("@TourId", model.TourId ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ComboId", model.ComboId);
                        command.Parameters.AddWithValue("@DepartureDate", model.DepartureDate ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@NumPeople", model.NumPeople);
                        command.Parameters.AddWithValue("@TotalPrice", model.TotalPrice);
                        command.Parameters.AddWithValue("@SpecialRequests", (object)model.SpecialRequests ?? DBNull.Value);
                        command.Parameters.AddWithValue("@BookingStatus", "Pending");
                        command.Parameters.AddWithValue("@BookingDate", DateTime.Now);

                        // Lấy bookingId từ cơ sở dữ liệu
                        int bookingId = (int)command.ExecuteScalar();
                        return bookingId;
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("Lỗi SQL: " + sqlEx.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("Có lỗi xảy ra khi lưu dữ liệu vào cơ sở dữ liệu: " + ex.Message);
            }
        }
        private int GetAvailableSlots(int comboId, DateTime? departureDate)
        {
            int avai_slot = 0;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = @"
                SELECT 
                    (tcd.available_slots - COALESCE(SUM(b.num_people), 0)) AS remaining_slots
                FROM 
                    TourComboDeparture tcd
                LEFT JOIN 
                    Booking b ON tcd.combo_id = b.combo_id 
                    AND tcd.departure_date = b.departure_date
                WHERE 
                    tcd.combo_id = @combo_id 
                    AND tcd.departure_date = @departureDate
                GROUP BY 
                    tcd.available_slots;
                ";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@combo_id", comboId);
                    command.Parameters.AddWithValue("@departureDate", departureDate);
                    connection.Open();
                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        avai_slot = Convert.ToInt32(result);
                    }
                }
            }

            return avai_slot; // Trả về số chỗ trống còn lại
        }
        private string GetTourName(int comboId)
        {
            using (var context = new TourDBDataContext(connectionString))
            {
                // Lấy tên tour từ bảng TourCombo
                var tourName = (from c in context.TourCombos
                                where c.combo_id == comboId
                                select c.combo_name).FirstOrDefault();

                return tourName ?? "Không thấy tên"; // Nếu không tìm thấy, trả về chuỗi rỗng
            }
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