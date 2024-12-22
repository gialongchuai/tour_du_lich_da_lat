using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using DaLatTour.Models;
using System.Web.Mvc;
using System.Linq;
using System.IO;
using System.Data.Entity;
using System.Web.Configuration;


namespace DaLatTour.Controllers
{
    public class TourController : Controller
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        TourDBDataContext db = new TourDBDataContext(connectionString);

        public ActionResult Index(int page = 1)
        {
            // Gọi hàm để lấy tất cả các tour
            IEnumerable<Tour> tours = GetAllTours().ToList(); // Chuyển đổi thành danh sách để lấy tổng số tour
            int pageSize = 9; // Số lượng tour trên mỗi trang
            int totalTours = tours.Count(); // Tổng số tour
            var pagedTours = tours.Skip((page - 1) * pageSize).Take(pageSize); // Phân trang

            ViewBag.TotalTours = totalTours; // Lưu tổng số tour vào ViewBag để sử dụng trong view
            ViewBag.CurrentPage = page; // Lưu trang hiện tại vào ViewBag

            return View(pagedTours); // Trả về view với danh sách tours đã phân trang
        }


        // Hàm để hiển thị tour đánh giá cao nhất
        public ActionResult TopRated()
        {
            IEnumerable<Tour> topRatedTours = TopRatedTours();
            return View(topRatedTours); // Trả về view với danh sách tour đánh giá cao nhất
        }


        public ActionResult TourReview()
        {
            IEnumerable<TourReview> tourReviews = GetAllReviews();
            return View(tourReviews);
        }

        public IEnumerable<TourReview> GetAllReviews()
        {
            List<TourReview> reviews = new List<TourReview>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Truy vấn dữ liệu kết hợp từ bảng Tour_Review, Customer, và Tour
                string sql = @"SELECT tr.review_id, tr.review_text, tr.rating, tr.review_date, 
                                      u.customer_id, u.name AS customer_name, u.email, u.phone,
                                      t.tour_id, t.tour_name, tour_image
                               FROM Tour_Review tr
                               JOIN Customer u ON tr.customer_id = u.customer_id
                               JOIN Tour t ON tr.tour_id = t.tour_id";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        // Tạo đối tượng TourReview và liên kết thông tin User và Tour
                        var review = new TourReview
                        {
                            review_id = (int)reader["review_id"],
                            review_text = reader["review_text"].ToString(),
                            rating = (int)reader["rating"],
                            review_date = (DateTime)reader["review_date"],
                            Customer = new User
                            {
                                customer_id = (int)reader["customer_id"],
                                name = reader["customer_name"].ToString(),
                                email = reader["email"].ToString(),
                                phone = reader["phone"].ToString()
                            },
                            Tour = new DaLatTour.Models.Tour
                            {
                                tour_id = (int)reader["tour_id"],
                                tour_name = reader["tour_name"].ToString(),
                                tour_image = reader["tour_image"].ToString()
                            }
                        };

                        reviews.Add(review); // Thêm đánh giá vào danh sách
                    }
                }
            }

            return reviews;
        }

        [HttpPost]
        public JsonResult SaveReview(int TourId, int Rating, string ReviewText)
        {
            try
            {
                // Lấy CustomerId từ session
                int customerId = Convert.ToInt32(Session["Customer_Id"]);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string insertQuery = @"INSERT INTO Tour_Review (customer_id, tour_id, review_text, rating, review_date) 
                                 VALUES (@CustomerId, @TourId, @ReviewText, @Rating, @ReviewDate)";

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        // Thêm parameters để tránh SQL injection
                        command.Parameters.AddWithValue("@CustomerId", customerId);
                        command.Parameters.AddWithValue("@TourId", TourId);
                        command.Parameters.AddWithValue("@ReviewText", string.IsNullOrEmpty(ReviewText) ? (object)DBNull.Value : ReviewText);
                        command.Parameters.AddWithValue("@Rating", Rating);
                        command.Parameters.AddWithValue("@ReviewDate", DateTime.Now);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            return Json(new { success = true, message = "Đánh giá tour đã được lưu thành công!" });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Không thể lưu đánh giá tour." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpGet]
        public JsonResult CheckLoginStatus()
        {
            bool isLoggedIn = Session["Customer_Id"] != null;
            return Json(new { isLoggedIn = isLoggedIn }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetTours()
        {
            var tours = db.Tours.Select(t => new
            {
                TourId = t.tour_id,
                TourName = t.tour_name,
                TourImage = t.tour_image // Bao gồm hình ảnh
            }).ToList();

            return Json(tours, JsonRequestBehavior.AllowGet);
        }



        public ActionResult TourDetail(int id)
        {
            TourDetail tourDetail = GetTourDetailById(id);
            return View(tourDetail); // Trả về view với danh sách tour đánh giá cao nhất
        }


        private IEnumerable<Tour> GetAllTours() // Trả về IEnumerable<Tour>
        {
            List<Tour> tours = new List<Tour>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "SELECT * FROM Tour"; // Truy vấn tất cả các cột trong bảng Tour
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string fullDescription = reader["description"].ToString();
                        string shortenedDescription = ShortenDescription(fullDescription, 40); // Cắt ngắn mô tả

                        // Kiểm tra nếu cột 'average_rating' tồn tại và không phải null
                        double averageRating = reader["average_rating"] != DBNull.Value
                            ? (double)reader["average_rating"]
                            : 0.0;

                        // Thêm đối tượng Tour vào danh sách
                        tours.Add(new Tour
                        {
                            tour_id = (int)reader["tour_id"],
                            tour_name = reader["tour_name"].ToString(),
                            description = shortenedDescription,
                            tour_image = reader["tour_image"].ToString(),
                            price = (decimal)reader["price"],
                            duration = (int)reader["duration"],
                            travelby = reader["travelby"].ToString(),
                            available_slots = (int)reader["available_slots"],
                            created_at = (DateTime)reader["created_at"],
                            average_rating = averageRating // Gán giá trị rating đã kiểm tra
                        });
                    }
                }
            }
            return tours; // Trả về IEnumerable<Tour>
        }

        private string ShortenDescription(string description, int maxLength)
        {
            if (string.IsNullOrEmpty(description) || description.Length <= maxLength)
            {
                return description; // Nếu không có mô tả hoặc ngắn hơn maxLength, trả về nguyên văn
            }

            return description.Substring(0, maxLength) + "..."; // Cắt ngắn và thêm ba chấm
        }

        private IEnumerable<Tour> TopRatedTours() // Trả về IEnumerable<Tour>
        {
            List<Tour> tours = new List<Tour>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = @"SELECT T.tour_id, 
                               T.tour_name, 
                               CAST(T.description AS NVARCHAR(MAX)) AS description, 
                               T.tour_image,  
                               T.price, 
                               AVG(TR.rating) AS average_rating
                               FROM Tour T
                               JOIN Tour_Review TR ON T.tour_id = TR.tour_id 
                               GROUP BY T.tour_id, T.tour_name, CAST(T.description AS NVARCHAR(MAX)), T.tour_image, T.price 
                               ORDER BY average_rating DESC
                               OFFSET 0 ROWS FETCH NEXT 3 ROWS ONLY;";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string fullDescription = reader["description"].ToString();
                        string shortenedDescription = ShortenDescription(fullDescription, 40); // Cắt ngắn mô tả ở đây
                        tours.Add(new Tour
                        {
                            tour_id = (int)reader["tour_id"],
                            tour_name = reader["tour_name"].ToString(),
                            description = shortenedDescription,
                            tour_image = reader["tour_image"].ToString(),
                            price = (decimal)reader["price"], // Lấy giá tour
                            duration = 0, // Chưa có thông tin này trong truy vấn
                            travelby = string.Empty, // Không có thông tin này trong truy vấn
                            available_slots = 0, // Không có thông tin này trong truy vấn
                            created_at = DateTime.Now // Không có thông tin này trong truy vấn
                        });
                    }
                }
            }
            return tours; // Trả về IEnumerable<Tour>
        }
        private TourDetail GetTourDetailById(int tourId) // Trả về TourDetail
        {
            TourDetail tourDetail = null; // Khởi tạo biến tourDetail

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = @"SELECT TD.tour_detail_id, 
                              TD.tour_id, 
                              TD.image_url, 
                              TD.num_people, 
                              TD.departure_date, 
                              TD.return_date, 
                              TD.daily_activities, 
                              T.tour_name, 
                              T.description, 
                              T.tour_image, 
                              T.price
                       FROM Tour_Detail TD
                       JOIN Tour T ON TD.tour_id = T.tour_id
                       WHERE TD.tour_id = @tourId";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@tourId", tourId);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read()) // Kiểm tra có dữ liệu không
                    {
                        tourDetail = new TourDetail
                        {
                            tour_detail_id = (int)reader["tour_detail_id"],
                            tour_id = (int)reader["tour_id"],
                            image_url = reader["image_url"].ToString(),
                            num_people = (int)reader["num_people"],
                            departure_date = (DateTime)reader["departure_date"],
                            return_date = (DateTime)reader["return_date"],
                            daily_activities = reader["daily_activities"].ToString(),
                            Tour = new DaLatTour.Models.Tour // Chỉ rõ không gian tên ở đây
                            {
                                tour_name = reader["tour_name"].ToString(),
                                description = reader["description"].ToString(),
                                tour_image = reader["tour_image"].ToString(),
                                price = (decimal)reader["price"]
                            }
                        };
                    }
                }
            }
            return tourDetail; // Trả về TourDetail hoặc null nếu không tìm thấy
        }


        public ActionResult Search(string location, DateTime? startDate, DateTime? endDate, decimal? minPrice, decimal? maxPrice)
        {
            // Lấy tất cả các tour
            var tours = from tour in db.Tours
                        join tourDetail in db.Tour_Details on tour.tour_id equals tourDetail.tour_id
                        select new
                        {
                            tour,
                            tourDetail
                        };

            // Lọc theo địa điểm (tour_name)
            if (!string.IsNullOrEmpty(location))
            {
                tours = tours.Where(t => t.tour.description.Contains(location));
            }

            // Lọc theo ngày bắt đầu (startDate)
            if (startDate.HasValue)
            {
                tours = tours.Where(t => t.tourDetail.departure_date >= startDate);
            }

            // Lọc theo ngày kết thúc (endDate)
            if (endDate.HasValue)
            {
                tours = tours.Where(t => t.tourDetail.return_date <= endDate);
            }

            // Lọc theo giá tour
            if (minPrice.HasValue)
            {
                tours = tours.Where(t => t.tour.price >= minPrice);
            }
            if (maxPrice.HasValue)
            {
                tours = tours.Where(t => t.tour.price <= maxPrice);
            }

            // Chuyển đổi thành danh sách để hiển thị
            var result = tours.Select(t => t.tour).ToList();

            // Đưa dữ liệu về view
            return View("Index", result);  // Hiển thị lại view Index với kết quả đã tìm kiếm
        }
        public ActionResult BookTour(Booking booking)
        {
            if (ModelState.IsValid)
            {
                booking.booking_date = DateTime.Now;

                // Xác định trạng thái đặt tour dựa trên phương thức thanh toán
                if (Request.Form["payment_method"] == "cash")
                {
                    booking.booking_status = "Pending"; // Nếu là tiền mặt
                }
                else if (Request.Form["payment_method"] == "bank_transfer")
                {
                    booking.booking_status = "Confirmed"; // Nếu là chuyển khoản
                }

                // Kiểm tra Tour ID hợp lệ
                if (booking.tour_id.HasValue && booking.tour_id.Value > 0)
                {
                    SaveBooking(booking); // Lưu thông tin đặt tour
                    return RedirectToAction("Confirmation"); // Chuyển hướng đến trang cảm ơn
                }
                else
                {
                    ModelState.AddModelError("", "Tour ID is not valid.");
                }
            }

            // Nếu model không hợp lệ hoặc không có Tour ID hợp lệ, quay lại trang chi tiết tour
            TourDetail tourDetail = null;
            if (booking.tour_id.HasValue)
            {
                tourDetail = GetTourDetailById(booking.tour_id.Value); // Lấy thông tin chi tiết tour từ database
                if (tourDetail == null)
                {
                    return HttpNotFound(); // Nếu không tìm thấy tour, trả về 404
                }
            }

            return View("TourDetail", tourDetail); // Trả về view chi tiết tour với thông tin hợp lệ
        }


        private void SaveBooking(Booking booking)
        {
            if (booking.customer_id == 0) // Giả sử bạn có cách lấy customer_id và nó phải khác 0
            {
                // Thực hiện xử lý khi customer_id không hợp lệ
                throw new InvalidOperationException("Customer ID is not valid.");
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "INSERT INTO Booking (customer_id, tour_id, booking_date, total_price, num_people, booking_status, special_requests) " +
                             "VALUES (@CustomerId, @TourId, @BookingDate, @TotalPrice, @NumPeople, @BookingStatus, @SpecialRequests)";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@CustomerId", booking.customer_id);
                    command.Parameters.AddWithValue("@TourId", booking.tour_id);
                    command.Parameters.AddWithValue("@BookingDate", booking.booking_date);
                    command.Parameters.AddWithValue("@TotalPrice", booking.total_price);
                    command.Parameters.AddWithValue("@NumPeople", booking.num_people);
                    command.Parameters.AddWithValue("@BookingStatus", booking.booking_status);
                    command.Parameters.AddWithValue("@SpecialRequests", booking.special_requests ?? (object)DBNull.Value);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
        public ActionResult Confirmation()
        {
            return View();
        }


        // ham them tour moi
        public void AddTour(Tour tour) // Thêm mới một tour
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "INSERT INTO Tour (tour_name, description, tour_image, price, duration, travelby, available_slots) " +
                             "VALUES (@tour_name, @description, @tour_image, @price, @duration, @travelby, @available_slots)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    // Gán giá trị cho các tham số
                    command.Parameters.AddWithValue("@tour_name", tour.tour_name);
                    command.Parameters.AddWithValue("@description", string.IsNullOrEmpty(tour.description) ? DBNull.Value : (object)tour.description);
                    command.Parameters.AddWithValue("@tour_image", tour.tour_image);
                    command.Parameters.AddWithValue("@price", tour.price);
                    command.Parameters.AddWithValue("@duration", tour.duration);
                    command.Parameters.AddWithValue("@travelby", tour.travelby);
                    command.Parameters.AddWithValue("@available_slots", tour.available_slots);

                    connection.Open();
                    command.ExecuteNonQuery(); // Thực thi lệnh SQL để thêm tour vào DB
                }
            }
        }


        [HttpPost]
        public JsonResult Create(Tour tour)
        {
            try
            {
                var file = Request.Files["tour_image"];
                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/Asset/img"), fileName);
                    file.SaveAs(path);
                    tour.tour_image = fileName; // Lưu tên tệp vào thuộc tính tour_image
                }

                AddTour(tour); // Gọi hàm AddTour để thêm tour vào cơ sở dữ liệu
                return Json(new { success = true, message = "Tour đã được thêm thành công!" });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi
                return Json(new { success = false, message = "Đã có lỗi xảy ra: " + ex.Message });
            }
        }

        // Hàm xóa tour theo tour_id
        [HttpPost]
        public ActionResult DeleteTour(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "DELETE FROM Tour WHERE tour_id = @tour_id";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    // Gán giá trị cho tham số @tour_id
                    command.Parameters.AddWithValue("@tour_id", id);

                    connection.Open();

                    try
                    {
                        int rowsAffected = command.ExecuteNonQuery(); // Thực thi lệnh SQL để xóa tour khỏi DB

                        // Kiểm tra xem có hàng nào bị ảnh hưởng không, nếu có thì trả về kết quả thành công
                        if (rowsAffected > 0)
                        {
                            return Json(new { success = true, message = "Xóa tour thành công." }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { success = false, message = "Tour không tồn tại." }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (SqlException ex)
                    {
                        // Logging lỗi để dễ dàng gỡ lỗi
                        // Bạn có thể sử dụng một thư viện logging như NLog hoặc log4net
                        // Ví dụ: Log.Error(ex, "Lỗi khi xóa tour với tour_id: {0}", id);

                        // Kiểm tra mã lỗi để xác định nguyên nhân
                        if (ex.Number == 547) // Lỗi khóa ngoại
                        {
                            return Json(new { success = false, message = "Không thể xóa tour này vì nó đã được liên kết với các bảng khác." }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { success = false, message = "Đã xảy ra lỗi không xác định." }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        public Tour GetTourById(int id) // Lấy thông tin tour theo ID
        {
            Tour tour = null; // Khởi tạo biến tour

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "SELECT tour_name, description, tour_image, price, duration, travelby, available_slots FROM Tour WHERE tour_id = @tour_id";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    // Gán giá trị cho tham số
                    command.Parameters.AddWithValue("@tour_id", id);

                    connection.Open(); // Mở kết nối

                    using (SqlDataReader reader = command.ExecuteReader()) // Thực thi lệnh SQL và đọc dữ liệu
                    {
                        if (reader.Read()) // Nếu có kết quả
                        {
                            tour = new Tour
                            {
                                tour_name = reader["tour_name"].ToString(),
                                description = reader["description"] as string,
                                tour_image = reader["tour_image"].ToString(),
                                price = (decimal)reader["price"],
                                duration = (int)reader["duration"],
                                travelby = reader["travelby"].ToString(),
                                available_slots = (int)reader["available_slots"]
                            };
                        }
                    }
                }
            }

            return tour; // Trả về đối tượng tour
        }

        [HttpGet]
        public JsonResult GetTour(int id)
        {
            try
            {
                Tour tour = GetTourById(id); // Gọi phương thức GetTour để lấy thông tin tour

                if (tour == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy tour!" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = true,
                    tour_name = tour.tour_name,
                    description = tour.description,
                    tour_image = tour.tour_image,
                    price = tour.price,
                    duration = tour.duration,
                    travelby = tour.travelby,
                    available_slots = tour.available_slots
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã có lỗi xảy ra: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        //Cap nhat tour
        public void UpdateTour(Tour tour) // Hàm cập nhật tour
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "UPDATE Tour SET tour_name = @tour_name, description = @description, tour_image = @tour_image, " +
                             "price = @price, duration = @duration, travelby = @travelby, available_slots = @available_slots " +
                             "WHERE tour_id = @tour_id"; // Thêm điều kiện WHERE để xác định tour cần sửa

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    // Gán giá trị cho các tham số
                    command.Parameters.AddWithValue("@tour_name", tour.tour_name);
                    command.Parameters.AddWithValue("@description", string.IsNullOrEmpty(tour.description) ? DBNull.Value : (object)tour.description);
                    command.Parameters.AddWithValue("@tour_image", tour.tour_image);
                    command.Parameters.AddWithValue("@price", tour.price);
                    command.Parameters.AddWithValue("@duration", tour.duration);
                    command.Parameters.AddWithValue("@travelby", tour.travelby);
                    command.Parameters.AddWithValue("@available_slots", tour.available_slots);
                    command.Parameters.AddWithValue("@tour_id", tour.tour_id); // Gán giá trị cho tour_id để cập nhật đúng tour

                    connection.Open();
                    command.ExecuteNonQuery(); // Thực thi lệnh SQL để cập nhật tour trong DB
                }
            }
        }

        [HttpGet]
        public JsonResult SearchTours(string searchTerm)
        {
            List<Tour> tours = new List<Tour>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = @"
            SELECT * FROM Tour 
            WHERE tour_name COLLATE Latin1_General_CI_AI LIKE @searchTerm 
               OR description COLLATE Latin1_General_CI_AI LIKE @searchTerm 
               OR CAST(price AS NVARCHAR) LIKE @searchTerm
               OR CAST(duration AS NVARCHAR) LIKE @searchTerm
               OR travelby COLLATE Latin1_General_CI_AI LIKE @searchTerm
               OR CAST(available_slots AS NVARCHAR) LIKE @searchTerm";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@searchTerm", "%" + searchTerm + "%");
                    connection.Open();

                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        tours.Add(new Tour
                        {
                            tour_id = (int)reader["tour_id"],
                            tour_name = reader["tour_name"].ToString(),
                            description = reader["description"].ToString(),
                            tour_image = reader["tour_image"].ToString(),
                            price = (decimal)reader["price"],
                            duration = (int)reader["duration"],
                            travelby = reader["travelby"].ToString(),
                            available_slots = (int)reader["available_slots"],
                            created_at = (DateTime)reader["created_at"],
                            average_rating = reader["average_rating"] != DBNull.Value ? (double)reader["average_rating"] : 0.0
                        });
                    }
                }
            }

            return Json(tours, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Update(Tour tour)
        {
            try
            {
                var file = Request.Files["tour_image"];
                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/Asset/img"), fileName);
                    file.SaveAs(path);
                    tour.tour_image = fileName; // Lưu tên tệp vào thuộc tính tour_image
                }
                else
                {
                    // Nếu không có ảnh mới, giữ lại ảnh hiện tại
                    tour.tour_image = Request["tour_image_current"];
                }

                UpdateTour(tour); // Gọi hàm UpdateTour để cập nhật tour trong cơ sở dữ liệu
                return Json(new { success = true, message = "Tour đã được cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi
                return Json(new { success = false, message = "Đã có lỗi xảy ra: " + ex.Message });
            }
        }

        public ActionResult TourHistory()
        {
            if (Session["Customer_Id"] != null && int.TryParse(Session["Customer_Id"].ToString(), out int customerId))
            {
                var bookings = db.Bookings
                    .Where(b => b.customer_id == customerId)
                    .Include(b => b.Tour)
                    .Include(b => b.TourCombo)
                    .Select(b => new BookingModel
                    {
                        BookingId = b.booking_id,
                        Combo_id = b.combo_id,
                        CustomerName = b.Customer.name,
                        BookingDate = b.booking_date,
                        TotalPrice = b.total_price,
                        NumPeople = b.num_people,
                        BookingStatus = b.booking_status,
                        DepartureDate = b.departure_date,
                        TourName = b.Tour != null ? b.Tour.tour_name : b.TourCombo.combo_name,
                        IsReview = b.IsReview
                    })
                    .ToList();

                return View(bookings);
            }
            else
            {
                return RedirectToAction("TopRated", "Tour");
            }
        }
        [HttpPost]
        public ActionResult CancelBooking(int bookingId)
        {
            var booking = db.Bookings.FirstOrDefault(b => b.booking_id == bookingId);

            if (booking == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin đặt tour." });
            }

            // Kiểm tra điều kiện hủy trước ngày khởi hành 7 ngày
            if (booking.departure_date.HasValue && (booking.departure_date.Value - DateTime.Now).TotalDays >= 7)
            {
                booking.booking_status = "Cancelled";
                db.SubmitChanges();

                return Json(new { success = true, message = "Hủy tour thành công." });
            }
            else
            {
                return Json(new { success = false, message = "Chỉ có thể hủy tour trước ngày khởi hành 1 tuần." });
            }
        }

        /// <summary>
        /// Hiển thị trang quản lý lịch sử đặt tour và đánh giá tour
        /// </summary>
        /// <returns>View</returns>
        public ActionResult ReviewTours()
        {
            // Kiểm tra nếu Session["Customer_Id"] không null và có thể ép kiểu sang int
            if (Session["Customer_Id"] != null && int.TryParse(Session["Customer_Id"].ToString(), out int customerId))
            {
                var completedBookings = db.Bookings
                    .Where(b => b.customer_id == customerId && b.booking_status == "Confirmed" && b.IsReview ==1)
                    .Select(b => new BookingModel
                    {
                        BookingId = b.booking_id,
                        Combo_id = b.Tour != null ? b.Tour.tour_id : b.TourCombo.combo_id ,  // Lấy combo_id nếu có TourCombo
                        TourName = b.Tour != null ? b.Tour.tour_name : (b.TourCombo != null ? b.TourCombo.combo_name : "N/A") // Lấy tên tour hoặc combo_name nếu có
                    }).ToList();

                // Debug thông tin danh sách tour đã hoàn thành
                if (!completedBookings.Any())
                {
                    System.Diagnostics.Debug.WriteLine("Không tìm thấy tour nào");
                }
                else
                {
                    foreach (var booking in completedBookings)
                    {
                        System.Diagnostics.Debug.WriteLine($"Tour ID: {booking.Combo_id}, Tour Name: {booking.TourName}");
                    }
                }

                return View(completedBookings);
            }
            else
            {
                // Nếu không tìm thấy hoặc không ép kiểu được, redirect về trang khác
                return RedirectToAction("TopRated", "Tour");
            }
        }



        /// <summary>
        /// Nhận đánh giá của người dùng và lưu vào cơ sở dữ liệu
        /// </summary>
        /// <param name="tourId">Mã Tour</param>
        /// <param name="reviewContent">Nội dung đánh giá</param>
        /// <param name="rating">Điểm đánh giá</param>
        /// <returns>View</returns>
        [HttpPost]
        public ActionResult SubmitReview(int bookingId, string reviewContent, int rating)
        {
            if (Session["Customer_Id"] != null && int.TryParse(Session["Customer_Id"].ToString(), out int customerId))
            {
                // Tìm booking dựa trên bookingId và customerId
                var booking = db.Bookings.FirstOrDefault(b => b.booking_id == bookingId && b.customer_id == customerId && b.IsReview == 0);

                if (booking != null)
                {
                    // Tạo đối tượng đánh giá mới
                    var review = new Tour_Review
                    {
                        customer_id = customerId,
                        tour_id = booking.Tour != null ? booking.Tour.tour_id : (int?)null,  
                        combo_id = booking.Tour == null && booking.TourCombo != null ? booking.TourCombo.combo_id : (int?)null, 
                        review_text = reviewContent,
                        review_date = DateTime.Now,
                        rating = rating
                    };



                    // Cập nhật trạng thái IsReview của booking thành 1 (đã đánh giá)
                    booking.IsReview = 1;

                    // Chèn review vào cơ sở dữ liệu
                    db.Tour_Reviews.InsertOnSubmit(review);
                    db.SubmitChanges();  // Lưu thay đổi vào cơ sở dữ liệu

                    TempData["Message"] = "Đánh giá của bạn đã được gửi thành công!";
                    return RedirectToAction("TourHistory");  // Chuyển hướng về trang lịch sử tour
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy booking này hoặc bạn đã đánh giá tour này rồi!";
                }
            }

            return RedirectToAction("TopRated", "Tour");  // Nếu không có session hoặc không tìm thấy booking, chuyển hướng tới trang TopRated
        }

    }


}


