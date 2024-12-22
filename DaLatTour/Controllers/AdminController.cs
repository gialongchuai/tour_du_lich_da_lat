using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using DaLatTour.Models;
using DaLatTour.Athen;
using System.Linq;
using System.IO;
using System.Web;
using System.Data;
using ClosedXML.Excel;

namespace DaLatTour.Controllers
{
    [AuthorizeAdmin]
    public class AdminController : Controller
    {

        private static string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        TourDBDataContext db = new TourDBDataContext(connectionString);

        public ActionResult Admin()
        {
            var AdminRole = Session["AdminRole"]?.ToString();
            ViewBag.AdminRole = AdminRole;

            return View("Admin"); // Trả về view với danh sách tours
        }

        public ActionResult GetAllTour()
        {
            IEnumerable<Tour> tours = GetAllTours(); // Sử dụng IEnumerable
            return View(tours); // Trả về view với danh sách tours
        }

        private IEnumerable<Tour> GetAllTours() // Trả về IEnumerable<Tour>
        {
            List<Tour> tours = new List<Tour>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "SELECT * FROM Tour";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
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
                            created_at = (DateTime)reader["created_at"]
                        });
                    }
                }
            }

            // Lấy tên và vai trò của người dùng từ session
            var AdminEmail = Session["AdminEmail"]?.ToString();
            var AdminRole = Session["AdminRole"]?.ToString();
            var AdminName = Session["AdminName"]?.ToString(); // Lấy tên từ session

            // Lưu vai trò và tên vào ViewBag để sử dụng trong view
            ViewBag.AdminRole = AdminRole;
            ViewBag.AdminRole = AdminRole;

            return tours; // Trả về IEnumerable<Tour>
        }

        public ActionResult GetAllUser()
        {
            IEnumerable<User> users = GetAllUsers(); // Sử dụng IEnumerable
            return View(users); // Trả về view với danh sách tours
        }

        private IEnumerable<User> GetAllUsers() // Trả về IEnumerable<User>
        {
            List<User> users = new List<User>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "SELECT * FROM Customer"; // Truy vấn lấy tất cả khách hàng
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            customer_id = (int)reader["customer_id"],
                            name = reader["name"].ToString(),
                            email = reader["email"].ToString(),
                            phone = reader["phone"].ToString(),
                            username = reader["username"].ToString(),
                            password = reader["password"].ToString(),
                            address = reader["address"]?.ToString(), // Có thể để trống
                            dob = reader["dob"] != DBNull.Value ? (DateTime?)reader["dob"] : null, // Nullable DateTime
                            created_at = (DateTime)reader["created_at"] // Ngày tạo tài khoản
                        });
                    }
                }
            }

            // Lấy tên và vai trò của người dùng từ session
            var AdminEmail = Session["AdminEmail"]?.ToString();
            var AdminRole = Session["AdminRole"]?.ToString();
            var AdminName = Session["AdminName"]?.ToString(); // Lấy tên từ session

            // Lưu vai trò và tên vào ViewBag để sử dụng trong view
            ViewBag.AdminRole = AdminRole;
            ViewBag.AdminRole = AdminRole;

            return users; // Trả về IEnumerable<User>
        }

        public ActionResult GetAllStaff()
        {
            IEnumerable<Staff> staffs = GetAllStaffs();
            return View(staffs);
        }

        private IEnumerable<Staff> GetAllStaffs()
        {
            List<Staff> staffList = new List<Staff>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "SELECT * FROM Staff";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        staffList.Add(new Staff
                        {
                            staff_id = (int)reader["staff_id"],
                            name = reader["name"].ToString(),
                            email = reader["email"].ToString(),
                            phone = reader["phone"] != DBNull.Value ? reader["phone"].ToString() : null,
                            role = reader["role"].ToString(),
                            password = reader["password"].ToString(),
                            created_at = (DateTime)reader["created_at"]
                        });
                    }
                }
            }

            // Lấy tên và vai trò của người dùng từ session
            var AdminEmail = Session["AdminEmail"]?.ToString();
            var AdminRole = Session["AdminRole"]?.ToString();
            var AdminName = Session["AdminName"]?.ToString(); // Lấy tên từ session

            // Lưu vai trò và tên vào ViewBag để sử dụng trong view
            ViewBag.AdminRole = AdminRole;
            ViewBag.AdminRole = AdminRole;

            return staffList;
        }

        // Action để nhận yêu cầu từ Ajax và thêm người dùng mới
        [HttpPost]
        public ActionResult CreateUser(string name, string email, string phone, string username, string password, string address, DateTime dob)
        {
            try
            {
                // Chuỗi kết nối SQL Server
                string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Câu lệnh SQL để thêm người dùng
                    string query = "INSERT INTO Customer (name, email, phone, username, password, address, dob) " +
                                   "VALUES (@Name, @Email, @Phone, @Username, @Password, @Address, @Dob)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Thêm tham số
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Phone", phone);
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", password);
                        cmd.Parameters.AddWithValue("@Address", address);
                        cmd.Parameters.AddWithValue("@Dob", dob);

                        // Thực thi câu lệnh
                        cmd.ExecuteNonQuery();
                    }
                }

                // Trả về thông báo thành công
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và trả về thông báo lỗi
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Hàm xóa user theo customer_id
        [HttpPost]
        public ActionResult DeleteUser(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "DELETE FROM Customer WHERE customer_id = @customer_id";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    // Gán giá trị cho tham số @customer_id
                    command.Parameters.AddWithValue("@customer_id", id);

                    connection.Open();

                    try
                    {
                        int rowsAffected = command.ExecuteNonQuery(); // Thực thi lệnh SQL để xóa user khỏi DB

                        // Kiểm tra xem có hàng nào bị ảnh hưởng không, nếu có thì trả về kết quả thành công
                        if (rowsAffected > 0)
                        {
                            return Json(new { success = true, message = "Xóa người dùng thành công." }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { success = false, message = "Người dùng không tồn tại." }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 547) // Lỗi khóa ngoại
                        {
                            return Json(new { success = false, message = "Không thể xóa người dùng này vì nó đã được liên kết với các bảng khác." }, JsonRequestBehavior.AllowGet);
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

        public ActionResult Statistical(string timeRange = "month", DateTime? startDate = null, DateTime? endDate = null)
        {
            // Thiết lập mặc định nếu không có ngày bắt đầu/kết thúc
            startDate = startDate ?? new DateTime(DateTime.Now.Year, 1, 1); // Bắt đầu từ đầu năm
            endDate = endDate ?? DateTime.Now; // Kết thúc tại ngày hiện tại

            // Lấy dữ liệu Booking từ cơ sở dữ liệu
            var revenueData = db.Bookings
                                .Where(b => b.booking_date >= startDate && b.booking_date <= endDate)
                                .ToList();

            // Dữ liệu doanh thu theo khoảng thời gian
            var groupedData = new List<dynamic>();

            switch (timeRange.ToLower())
            {
                case "day":
                    groupedData = revenueData.GroupBy(b => b.booking_date.Value.Date)
                                             .Select(g => new
                                             {
                                                 TimeLabel = g.Key.ToString("dd/MM/yyyy"),
                                                 TotalRevenue = g.Sum(b => b.total_price)
                                             })
                                             .ToList<dynamic>();
                    break;
                case "week":
                    groupedData = revenueData.GroupBy(b => System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                                                b.booking_date.Value,
                                                System.Globalization.CalendarWeekRule.FirstDay,
                                                DayOfWeek.Monday))
                                             .Select(g => new
                                             {
                                                 TimeLabel = $"Tuần {g.Key}",
                                                 TotalRevenue = g.Sum(b => b.total_price)
                                             })
                                             .ToList<dynamic>();
                    break;
                case "month":
                    groupedData = revenueData.GroupBy(b => b.booking_date.Value.ToString("MM/yyyy"))
                                             .Select(g => new
                                             {
                                                 TimeLabel = g.Key,
                                                 TotalRevenue = g.Sum(b => b.total_price)
                                             })
                                             .ToList<dynamic>();
                    break;
                case "quarter":
                    groupedData = revenueData.GroupBy(b => (b.booking_date.Value.Month - 1) / 3 + 1)
                                             .Select(g => new
                                             {
                                                 TimeLabel = $"Quý {g.Key}",
                                                 TotalRevenue = g.Sum(b => b.total_price)
                                             })
                                             .ToList<dynamic>();
                    break;
                case "year":
                    groupedData = revenueData.GroupBy(b => b.booking_date.Value.Year)
                                             .Select(g => new
                                             {
                                                 TimeLabel = g.Key.ToString(),
                                                 TotalRevenue = g.Sum(b => b.total_price)
                                             })
                                             .ToList<dynamic>();
                    break;
                default:
                    groupedData = revenueData.GroupBy(b => b.booking_date.Value.ToString("MM/yyyy"))
                                             .Select(g => new
                                             {
                                                 TimeLabel = g.Key,
                                                 TotalRevenue = g.Sum(b => b.total_price)
                                             })
                                             .ToList<dynamic>();
                    break;
            }

            // Lấy dữ liệu số lượng khách theo từng mốc thời gian
            var customerData = revenueData.GroupBy(b => b.booking_date.Value.ToString("MM/yyyy"))
                                          .Select(g => new
                                          {
                                              TimeLabel = g.Key,
                                              TotalCustomers = g.Sum(b => b.num_people)
                                          })
                                          .ToList();

            // Đưa dữ liệu doanh thu và số khách vào ViewBag
            ViewBag.Labels = groupedData.Select(g => g.TimeLabel).ToList(); // Nhãn thời gian (ngày/tuần/tháng/quý/năm)
            ViewBag.Data = groupedData.Select(g => g.TotalRevenue).ToList(); // Doanh thu tương ứng
            ViewBag.Customers = customerData.Select(c => c.TotalCustomers).ToList(); // Tổng số khách tương ứng
            ViewBag.StartDate = startDate; // Ngày bắt đầu
            ViewBag.EndDate = endDate; // Ngày kết thúc
            ViewBag.TimeRange = timeRange; // Loại mốc thời gian (ngày, tuần, tháng, quý, năm)

            return View("Statistical");
        }

        public ActionResult TourStatis(DateTime? startDate, DateTime? endDate)
        {
            // Sử dụng ngày hiện tại nếu không có ngày bắt đầu và kết thúc
            startDate = startDate ?? DateTime.Now.AddMonths(-1).Date;
            endDate = endDate ?? DateTime.Now.Date;

            // Lấy dữ liệu số lượng tour và combo từ bảng Booking
            var tourStats = db.Bookings
                .Where(b => b.booking_date.HasValue &&
                            b.booking_date.Value.Date >= startDate.Value &&
                            b.booking_date.Value.Date <= endDate.Value)
                .GroupBy(b => b.Tour != null ? b.Tour.tour_name : b.TourCombo.combo_name)  // Nhóm theo tên Tour hoặc Combo
                .Select(g => new TourStatisticViewModel
                {
                    Name = g.Key,  // Tên Tour/Combo
                    TotalBookings = g.Count(),  // Số lượt đặt
                    TotalPeople = g.Sum(b => b.num_people),  // Tổng số khách
                    TotalRevenue = g.Sum(b => b.total_price)  // Tổng doanh thu
                })
                .OrderByDescending(g => g.TotalBookings)  // Sắp xếp giảm dần theo số lượt đặt
                .ToList();

            // Tính toán thêm các dữ liệu tổng quan:
            decimal totalRevenue = tourStats.Sum(t => t.TotalRevenue);  // Tổng doanh thu
            var mostBookedTour = tourStats.OrderByDescending(t => t.TotalBookings).FirstOrDefault()?.Name ?? "Không có";
            var mostPopularTour = tourStats.OrderByDescending(t => t.TotalPeople).FirstOrDefault()?.Name ?? "Không có";
            var totalCustomers = tourStats.Sum(t => t.TotalPeople);  // Tổng số khách hàng

            // Thêm thông tin bổ sung vào ViewBag để hiển thị trên giao diện
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.MostBookedTour = mostBookedTour;
            ViewBag.MostPopularTour = mostPopularTour;
            ViewBag.TotalCustomers = totalCustomers;  // Tổng số khách hàng
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            // Return the model (TourStats) to the view
            return View(tourStats);
        }







        [HttpGet]
        public ActionResult GetUser(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "SELECT * FROM Customer WHERE customer_id = @customer_id";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    // Gán giá trị cho tham số @customer_id
                    command.Parameters.AddWithValue("@customer_id", id);

                    connection.Open();

                    try
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var user = new User
                                {
                                    customer_id = (int)reader["customer_id"],
                                    name = reader["name"].ToString(),
                                    email = reader["email"].ToString(),
                                    phone = reader["phone"].ToString(),
                                    username = reader["username"].ToString(),
                                    password = reader["password"].ToString(),
                                    address = reader["address"].ToString(),
                                    dob = reader["dob"] != DBNull.Value ? (DateTime?)reader["dob"] : null,
                                };

                                return Json(new { success = true, data = user }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json(new { success = false, message = "Người dùng không tồn tại." }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine("Lỗi: " + ex.Message);
                        return Json(new { success = false, message = "Đã xảy ra lỗi không xác định." }, JsonRequestBehavior.AllowGet);
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        // Action để nhận yêu cầu từ Ajax và cập nhật thông tin người dùng
        [HttpPost]
        public ActionResult UpdateUser(int id, string name, string email, string phone, string username, string password, string address, DateTime dob)
        {
            try
            {
                // Chuỗi kết nối SQL Server
                string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Câu lệnh SQL để cập nhật người dùng
                    string query = "UPDATE Customer SET name = @Name, email = @Email, phone = @Phone, username = @Username, password = @Password, address = @Address, dob = @Dob " +
                                   "WHERE customer_id = @Id";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Thêm tham số
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Phone", phone);
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", password);
                        cmd.Parameters.AddWithValue("@Address", address);
                        cmd.Parameters.AddWithValue("@Dob", dob);

                        // Thực thi câu lệnh
                        cmd.ExecuteNonQuery();
                    }
                }

                // Trả về thông báo thành công
                return Json(new { success = true, message = "Người dùng đã được cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và trả về thông báo lỗi
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Hàm xóa user theo staff_id
        [HttpPost]
        public ActionResult DeleteStaff(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "DELETE FROM Staff WHERE staff_id = @staff_id";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    // Gán giá trị cho tham số @staff_id
                    command.Parameters.AddWithValue("@staff_id", id);

                    connection.Open();

                    try
                    {
                        int rowsAffected = command.ExecuteNonQuery(); // Thực thi lệnh SQL để xóa staff khỏi DB

                        // Kiểm tra xem có hàng nào bị ảnh hưởng không, nếu có thì trả về kết quả thành công
                        if (rowsAffected > 0)
                        {
                            return Json(new { success = true, message = "Xóa nhân viên thành công." }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { success = false, message = "Nhân viên không tồn tại." }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 547) // Lỗi khóa ngoại
                        {
                            return Json(new { success = false, message = "Không thể xóa nhân viên này vì nó đã được liên kết với các bảng khác." }, JsonRequestBehavior.AllowGet);
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

        // Action để nhận yêu cầu từ Ajax và thêm nhân viên mới
        [HttpPost]
        public ActionResult CreateStaff(string name, string email, string phone, string role, string password)
        {
            try
            {
                // Chuỗi kết nối SQL Server
                string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Câu lệnh SQL để thêm người dùng
                    string query = "INSERT INTO Staff (name, email, phone, role, password) " +
                                   "VALUES (@Name, @Email, @Phone, @Role, @Password)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Thêm tham số
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Phone", phone);
                        cmd.Parameters.AddWithValue("@Role", role);
                        cmd.Parameters.AddWithValue("@Password", password);

                        // Thực thi câu lệnh
                        cmd.ExecuteNonQuery();
                    }
                }

                // Trả về thông báo thành công
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và trả về thông báo lỗi
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public ActionResult GetStaff(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "SELECT * FROM Staff WHERE staff_id = @staff_id";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    // Gán giá trị cho tham số @customer_id
                    command.Parameters.AddWithValue("@staff_id", id);

                    connection.Open();

                    try
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var user = new User
                                {
                                    customer_id = (int)reader["staff_id"],
                                    name = reader["name"].ToString(),
                                    email = reader["email"].ToString(),
                                    phone = reader["phone"].ToString(),
                                    username = reader["role"].ToString(),
                                    password = reader["password"].ToString(),
                                };

                                return Json(new { success = true, data = user }, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                return Json(new { success = false, message = "Nhân viên không tồn tại." }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine("Lỗi: " + ex.Message);
                        return Json(new { success = false, message = "Đã xảy ra lỗi không xác định." }, JsonRequestBehavior.AllowGet);
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        // Action để nhận yêu cầu từ Ajax và cập nhật thông tin nhân viên
        [HttpPost]
        public ActionResult UpdateStaff(int id, string name, string email, string phone, string role, string password)
        {
            try
            {
                // Chuỗi kết nối SQL Server
                string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Câu lệnh SQL để cập nhật nhân viên
                    string query = "UPDATE Staff SET name = @Name, email = @Email, phone = @Phone, role = @Role, password = @Password " +
                                   "WHERE staff_id = @Id";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Thêm tham số
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Phone", phone);
                        cmd.Parameters.AddWithValue("@Role", role);
                        cmd.Parameters.AddWithValue("@Password", password);

                        // Thực thi câu lệnh
                        cmd.ExecuteNonQuery();
                    }
                }

                // Trả về thông báo thành công
                return Json(new { success = true, message = "Nhân viên đã được cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và trả về thông báo lỗi
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult SearchCustomers(string searchTerm)
        {
            List<User> customers = new List<User>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = @"
            SELECT * FROM Customer 
            WHERE name COLLATE Latin1_General_CI_AI LIKE @searchTerm 
               OR email COLLATE Latin1_General_CI_AI LIKE @searchTerm 
               OR phone COLLATE Latin1_General_CI_AI LIKE @searchTerm 
               OR username COLLATE Latin1_General_CI_AI LIKE @searchTerm 
               OR address COLLATE Latin1_General_CI_AI LIKE @searchTerm 
               OR CAST(dob AS NVARCHAR) LIKE @searchTerm";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@searchTerm", "%" + searchTerm + "%");
                    connection.Open();

                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        customers.Add(new User
                        {
                            customer_id = (int)reader["customer_id"],
                            name = reader["name"].ToString(),
                            email = reader["email"].ToString(),
                            phone = reader["phone"].ToString(),
                            username = reader["username"].ToString(),
                            password = reader["password"].ToString(),
                            address = reader["address"]?.ToString(),
                            dob = reader["dob"] != DBNull.Value ? (DateTime?)reader["dob"] : null,
                            created_at = (DateTime)reader["created_at"]
                        });
                    }
                }
            }

            return Json(customers, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult SearchStaffs(string searchTerm)
        {
            List<Staff> staffList = new List<Staff>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = @"
            SELECT * FROM Staff
            WHERE name COLLATE Latin1_General_CI_AI LIKE @searchTerm 
               OR email COLLATE Latin1_General_CI_AI LIKE @searchTerm 
               OR phone COLLATE Latin1_General_CI_AI LIKE @searchTerm 
               OR role COLLATE Latin1_General_CI_AI LIKE @searchTerm";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@searchTerm", "%" + searchTerm + "%");
                    connection.Open();

                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        staffList.Add(new Staff
                        {
                            staff_id = (int)reader["staff_id"],
                            name = reader["name"].ToString(),
                            email = reader["email"].ToString(),
                            phone = reader["phone"] != DBNull.Value ? reader["phone"].ToString() : null,
                            role = reader["role"].ToString(),
                            password = reader["password"].ToString(),
                            created_at = (DateTime)reader["created_at"]
                        });
                    }
                }
            }

            return Json(staffList, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Backup(string backupPath)
        {
            var AdminRole = Session["AdminRole"]?.ToString();
            // Kiểm tra nếu người dùng chưa nhập đường dẫn
            if (string.IsNullOrWhiteSpace(backupPath))
            {
                ViewBag.Message = "Lỗi: Vui lòng nhập đường dẫn sao lưu.";
                return View("Admin");
            }

            // Kiểm tra nếu đường dẫn không kết thúc bằng '.bak'
            if (!backupPath.EndsWith(".bak"))
            {
                // Kiểm tra nếu người dùng chỉ nhập đường dẫn thư mục, không có tên tệp
                if (System.IO.Directory.Exists(backupPath))
                {
                    // Thêm tên tệp mặc định vào đường dẫn
                    backupPath = System.IO.Path.Combine(backupPath, "DB_TourDuLich_Backup.bak");
                }
                else
                {
                    ViewBag.Message = "Lỗi: Đường dẫn không hợp lệ. Có thể thư mục không tồn tại hoặc tên tệp chưa đúng định dạng '.bak'.";
                    return View("Admin");
                }
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Tạo câu lệnh T-SQL để sao lưu
                    string backupQuery = $@"
                BACKUP DATABASE DB_TourDuLich 
                TO DISK = '{backupPath}' 
                WITH FORMAT, INIT, SKIP, NOREWIND, NOUNLOAD, STATS = 10;";

                    using (SqlCommand cmd = new SqlCommand(backupQuery, conn))
                    {
                        cmd.ExecuteNonQuery(); // Thực thi câu lệnh T-SQL
                    }
                }

                ViewBag.Message = "Sao lưu thành công!";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi: " + ex.Message);
                ViewBag.Message = "Đã xảy ra lỗi, hãy xem đường dẫn mẫu và kiểm tra lại!";
            }
            ViewBag.AdminRole = AdminRole;

            return View("Admin"); // Trả về View Admin.cshtml
        }




        [HttpPost]
        public ActionResult Restore(HttpPostedFileBase restoreFile)
        {
            var AdminRole = Session["AdminRole"]?.ToString();

            if (restoreFile != null && restoreFile.ContentLength > 0)
            {
                // Kiểm tra định dạng tệp
                if (Path.GetExtension(restoreFile.FileName).ToLower() != ".bak")
                {
                    ViewBag.Message = "Vui lòng chọn tệp phục hồi có định dạng '.bak'.";
                    return View("Admin"); // Trả về View Admin.cshtml
                }

                SqlConnection conn = null;
                try
                {
                    // Tạo một đường dẫn tạm trong thư mục C:\Temp
                    string tempPath = Path.Combine(@"C:\Users\gialo\Desktop", restoreFile.FileName);
                    restoreFile.SaveAs(tempPath);

                    conn = new SqlConnection(connectionString);
                    conn.Open();

                    // Ngắt kết nối tất cả người dùng khỏi cơ sở dữ liệu
                    string setSingleUserQuery = "ALTER DATABASE DB_TourDuLich SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";
                    using (SqlCommand cmd = new SqlCommand(setSingleUserQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Chuyển sang cơ sở dữ liệu master để thực hiện phục hồi
                    string useMasterDb = "USE master;";
                    using (SqlCommand cmd = new SqlCommand(useMasterDb, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Tạo câu lệnh T-SQL để phục hồi từ đường dẫn tạm thời
                    string restoreQuery = $@"
            RESTORE DATABASE DB_TourDuLich 
            FROM DISK = '{tempPath}' 
            WITH REPLACE, RECOVERY;";

                    using (SqlCommand cmd = new SqlCommand(restoreQuery, conn))
                    {
                        cmd.ExecuteNonQuery(); // Thực thi câu lệnh T-SQL phục hồi
                    }

                    ViewBag.Message = "Phục hồi thành công!";
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi: " + ex.Message);
                    ViewBag.Message = "Lỗi khi phục hồi, vui lòng kiểm tra lại!";
                }
                finally
                {
                    if (conn != null)
                    {
                        using (SqlCommand cmd = new SqlCommand("ALTER DATABASE DB_TourDuLich SET MULTI_USER;", conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                        conn.Close();
                    }
                }
            }
            else
            {
                ViewBag.Message = "Vui lòng chọn tệp phục hồi.";
            }
            ViewBag.AdminRole = AdminRole;

            return View("Admin"); // Trả về View Admin.cshtml
        }



        /// <summary>
        /// Trang thêm lịch trình cho các tour
        /// </summary>
        /// <returns></returns>
        public ActionResult TourDeparture()
        {
            var model = new TourSelectionViewModel
            {
                RegularTours = GetRegularTours(),
                ComboTours = GetComboTours()
            };
            // Lấy tên và vai trò của người dùng từ session
            var AdminEmail = Session["AdminEmail"]?.ToString();
            var AdminRole = Session["AdminRole"]?.ToString();
            var AdminName = Session["AdminName"]?.ToString(); // Lấy tên từ session

            // Lưu vai trò và tên vào ViewBag để sử dụng trong view
            ViewBag.AdminRole = AdminRole;
            ViewBag.AdminRole = AdminRole;
            return View(model);
        }

        /// <summary>
        /// lịch trình tour thường
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Tour_Detail> GetRegularTours()
        {
            var tours = new List<Tour_Detail>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Tour_Detail";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    tours.Add(new Tour_Detail
                    {
                        tour_detail_id = (int)reader["tour_detail_id"],
                        tour_id = (int)reader["tour_id"],
                        departure_date = (DateTime)reader["departure_date"],
                        return_date = (DateTime)reader["return_date"],
                        num_people = (int)reader["num_people"],
                        daily_activities = reader["daily_activities"].ToString()
                    });
                }
            }
            return tours;
        }

        /// <summary>
        /// lịch trình combo tour
        /// </summary>
        /// <returns></returns>
        private IEnumerable<TourComboDeparture> GetComboTours()
        {
            var tours = new List<TourComboDeparture>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM TourComboDeparture";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    tours.Add(new TourComboDeparture
                    {
                        departure_id = (int)reader["departure_id"],
                        combo_id = (int)reader["combo_id"],
                        departure_date = (DateTime)reader["departure_date"],
                        price = (decimal)reader["price"],
                        available_slots = (int)reader["available_slots"]
                    });
                }
            }
            return tours;
        }
        // Action để thêm lịch trình
        [HttpPost]
        public ActionResult AddSchedule(ScheduleViewModel scheduleData)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query;

                    if (scheduleData.TourId != null)
                    {
                        // Thêm lịch trình cho tour thường
                        query = "INSERT INTO Tour_Detail (tour_id, departure_date, return_date, num_people, daily_activities) VALUES (@TourId, @DepartureDate, @ReturnDate, @NumPeople, @DailyActivities)";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@TourId", scheduleData.TourId);
                            cmd.Parameters.AddWithValue("@DepartureDate", scheduleData.DepartureDate);
                            cmd.Parameters.AddWithValue("@ReturnDate", scheduleData.ReturnDate);
                            cmd.Parameters.AddWithValue("@NumPeople", scheduleData.AvailableSlots);
                            cmd.Parameters.AddWithValue("@DailyActivities", scheduleData.DailyActivities);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else if (scheduleData.ComboId != null)
                    {
                        // Thêm lịch trình cho combo tour
                        query = "INSERT INTO TourComboDeparture (combo_id, departure_date, price, available_slots) VALUES (@ComboId, @DepartureDate, @Price, @AvailableSlots)";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@ComboId", scheduleData.ComboId);
                            cmd.Parameters.AddWithValue("@DepartureDate", scheduleData.DepartureDate);
                            cmd.Parameters.AddWithValue("@Price", scheduleData.Price);
                            cmd.Parameters.AddWithValue("@AvailableSlots", scheduleData.AvailableSlots);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult UpdateSchedule(ScheduleViewModel scheduleData)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query;

                    if (scheduleData.TourId.HasValue)
                    {
                        // Cập nhật tour thường
                        query = "UPDATE Tour_Detail SET departure_date = @DepartureDate, return_date = @ReturnDate, num_people = @AvailableSlots, daily_activities = @DailyActivities WHERE tour_detail_id = @TourId";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@TourId", scheduleData.TourId);
                            cmd.Parameters.AddWithValue("@DepartureDate", scheduleData.DepartureDate);
                            cmd.Parameters.AddWithValue("@ReturnDate", scheduleData.ReturnDate);
                            cmd.Parameters.AddWithValue("@AvailableSlots", scheduleData.AvailableSlots);
                            cmd.Parameters.AddWithValue("@DailyActivities", scheduleData.DailyActivities ?? (object)DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else if (scheduleData.ComboId.HasValue)
                    {
                        // Cập nhật combo tour
                        query = "UPDATE TourComboDeparture SET departure_date = @DepartureDate, price = @Price, available_slots = @AvailableSlots WHERE departure_id = @ComboId";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@ComboId", scheduleData.ComboId);
                            cmd.Parameters.AddWithValue("@DepartureDate", scheduleData.DepartureDate);
                            cmd.Parameters.AddWithValue("@Price", scheduleData.Price ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@AvailableSlots", scheduleData.AvailableSlots);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "Invalid Tour Type or ID" });
                    }
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public ActionResult GetToursByType(string type)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = type == "Regular"
                        ? "SELECT tour_id AS id, tour_name AS name FROM Tour"
                        : "SELECT combo_id AS id, combo_name AS name FROM TourCombo";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        var tours = new List<object>();
                        while (reader.Read())
                        {
                            tours.Add(new { id = reader["id"], name = reader["name"] });
                        }

                        return Json(new { success = true, data = tours }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetScheduleDetail(int id, string tourType)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (tourType == "Regular")
                    {
                        // Lấy thông tin từ bảng Tour_Detail
                        string query = "SELECT * FROM Tour_Detail WHERE tour_detail_id = @Id";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);
                            SqlDataReader reader = cmd.ExecuteReader();
                            if (reader.Read())
                            {
                                var detail = new
                                {
                                    TourId = reader["tour_id"],
                                    DepartureDate = reader["departure_date"],
                                    ReturnDate = reader["return_date"],
                                    NumPeople = reader["num_people"],
                                    DailyActivities = reader["daily_activities"]
                                };
                                return Json(new { success = true, data = detail }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    else if (tourType == "Combo")
                    {
                        // Lấy thông tin từ bảng TourComboDeparture
                        string query = "SELECT * FROM TourComboDeparture WHERE departure_id = @Id";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", id);
                            SqlDataReader reader = cmd.ExecuteReader();
                            if (reader.Read())
                            {
                                var detail = new
                                {
                                    ComboId = reader["combo_id"],
                                    DepartureDate = reader["departure_date"],
                                    Price = reader["price"],
                                    AvailableSlots = reader["available_slots"]
                                };
                                return Json(new { success = true, data = detail }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
                return Json(new { success = false, message = "Không tìm thấy lịch trình" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DeleteRegularTour(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM Tour_Detail WHERE tour_detail_id = @Id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Lịch trình không tồn tại." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult DeleteComboTour(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM TourComboDeparture WHERE departure_id = @Id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Lịch trình không tồn tại." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult ExportRevenueToExcel(DateTime? startDate, DateTime? endDate)
        {
            DataTable revenueData = new DataTable();

            string query = @"
                    SELECT 
                        t.tour_name AS TourName,
                        tc.combo_name AS ComboName,
                        b.num_people AS NumberOfPeople,
                        b.total_price AS TotalPrice,
                        FORMAT(b.booking_date, 'MM/yyyy') AS MonthYear
                    FROM Booking b
                    LEFT JOIN Tour t ON b.tour_id = t.tour_id
                    LEFT JOIN TourCombo tc ON b.combo_id = tc.combo_id
                    WHERE (@StartDate IS NULL OR b.booking_date >= @StartDate)
                      AND (@EndDate IS NULL OR b.booking_date <= @EndDate);
                ";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StartDate", startDate ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@EndDate", endDate ?? (object)DBNull.Value);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(revenueData);
                    }
                }
            }

            var groupedData = revenueData.AsEnumerable()
                .GroupBy(row => new
                {
                    MonthYear = row["MonthYear"]?.ToString(),
                    TourName = row["TourName"]?.ToString(),
                    ComboName = row["ComboName"]?.ToString()
                })
                .Select(group => new
                {
                    MonthYear = group.Key.MonthYear,
                    TourName = group.Key.TourName,
                    ComboName = group.Key.ComboName,
                    TotalPeople = group.Sum(r => r["NumberOfPeople"] != DBNull.Value ? Convert.ToInt32(r["NumberOfPeople"]) : 0),
                    TotalPrice = group.Sum(r => r["TotalPrice"] != DBNull.Value ? Convert.ToDecimal(r["TotalPrice"]) : 0),
                })
                .OrderBy(x => DateTime.ParseExact(x.MonthYear, "MM/yyyy", null))
                .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Revenue Report");

                // Tiêu đề chính
                worksheet.Cell(1, 1).Value = "Công Ty Du Lịch Đà Lạt Discovery";
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 20;
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Range(1, 1, 1, 5).Merge();

                // Thông tin khoảng thời gian báo cáo
                worksheet.Cell(2, 1).Value = $"Báo Cáo Thống Kê Doanh Thu từ {startDate?.ToString("dd/MM/yyyy") ?? "N/A"} đến {endDate?.ToString("dd/MM/yyyy") ?? "N/A"}";
                worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(2, 1).Style.Font.FontSize = 13;
                worksheet.Range(2, 1, 2, 5).Merge();

                worksheet.Cell(3, 1).Value = $"Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy}";
                worksheet.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(3, 1).Style.Font.FontSize = 12;
                worksheet.Range(3, 1, 3, 5).Merge();

                // Tiêu đề các cột
                worksheet.Cell(5, 1).Value = "Tháng/Năm";
                worksheet.Cell(5, 2).Value = "Tour Thường";
                worksheet.Cell(5, 3).Value = "Tour theo gói";
                worksheet.Cell(5, 4).Value = "Tổng Số Người";
                worksheet.Cell(5, 5).Value = "Tổng Tiền";

                var headerRange = worksheet.Range(5, 1, 5, 5);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontSize = 12;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                int row = 6;
                decimal grandTotalPrice = 0;

                string currentMonthYear = string.Empty;
                int startMergeRow = row;

                for (int i = 0; i < groupedData.Count; i++)
                {
                    var item = groupedData[i];

                    if (item.MonthYear != currentMonthYear || i == groupedData.Count - 1)
                    {
                        if (i != 0 && currentMonthYear != string.Empty)
                        {
                            worksheet.Range(startMergeRow, 1, row - 1, 1).Merge();
                        }

                        worksheet.Cell(row, 1).Value = item.MonthYear;
                        currentMonthYear = item.MonthYear;
                        startMergeRow = row;
                    }

                    worksheet.Cell(row, 2).Value = item.TourName;
                    worksheet.Cell(row, 3).Value = item.ComboName;
                    worksheet.Cell(row, 4).Value = item.TotalPeople;
                    worksheet.Cell(row, 5).Value = item.TotalPrice;
                    worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";

                    grandTotalPrice += item.TotalPrice;

                    worksheet.Range(row, 1, row, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(row, 1, row, 5).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    row++;
                }

                worksheet.Range(startMergeRow, 1, row - 1, 1).Merge();

                // Tổng cộng
                worksheet.Cell(row, 4).Value = "Tổng Cộng";
                worksheet.Cell(row, 5).Value = grandTotalPrice;
                worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell(row, 5).Style.Font.Bold = true;

                worksheet.Range(row, 1, row, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;

                worksheet.Columns().AdjustToContents();

                // Căn giữa toàn bộ nội dung trên trang
                worksheet.PageSetup.CenterHorizontally = true; // Căn giữa theo chiều ngang
                worksheet.PageSetup.CenterVertically = true;  // Căn giữa theo chiều dọc

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    byte[] content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ThongKeDoanhThu.xlsx");
                }
            }
        }






    }
}