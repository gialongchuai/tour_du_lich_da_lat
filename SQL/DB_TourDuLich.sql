-- Tạo cơ sở dữ liệu
CREATE DATABASE DB_TourDuLich;
go

-- Sử dụng cơ sở dữ liệu
USE DB_TourDuLich;
GO

-- Bảng khách hàng (Customer)
CREATE TABLE Customer (
    customer_id INT PRIMARY KEY IDENTITY(1,1),   -- Mã khách hàng (khóa chính)
    name NVARCHAR(100) NOT NULL,                 -- Tên khách hàng
    email VARCHAR(100) NOT NULL UNIQUE,          -- Email (duy nhất)
    phone VARCHAR(15) NOT NULL,                  -- Số điện thoại
	username varchar(200) NOT NULL UNIQUE,			     -- tên dăng nhặp
    password NVARCHAR(255) NOT NULL,             -- Mật khẩu (mã hóa)
    address NVARCHAR(255),                       -- Địa chỉ (có thể để trống)
    dob DATE,                                    -- Ngày sinh (có thể để trống)
    created_at DATETIME DEFAULT GETDATE()        -- Ngày tạo tài khoản (mặc định hiện tại)
);
GO

-- Bảng nhân viên (Staff)
CREATE TABLE Staff (
    staff_id INT PRIMARY KEY IDENTITY(1,1),      -- Mã nhân viên (khóa chính)
    name NVARCHAR(100) NOT NULL,                  -- Tên nhân viên
    email VARCHAR(100) NOT NULL UNIQUE,          -- Email (duy nhất)
    phone VARCHAR(15),                           -- Số điện thoại (có thể để trống)
    role VARCHAR(50) CHECK (role IN ('Admin', 'Tour_Manager', 'Staff')) NOT NULL,  -- Vai trò
    password VARCHAR(255) NOT NULL,              -- Mật khẩu (mã hóa)
    created_at DATETIME DEFAULT GETDATE()        -- Ngày tạo tài khoản (mặc định hiện tại)
);
GO

CREATE TABLE Hotel (
    hotel_id INT PRIMARY KEY IDENTITY(1,1),        -- Mã khách sạn (khóa chính)
    hotel_name NVARCHAR(100) NOT NULL,             -- Tên khách sạn
    price_per_night DECIMAL(10, 2) NOT NULL,       -- Giá mỗi đêm
    location NVARCHAR(255),                        -- Địa chỉ khách sạn
    description NVARCHAR(MAX)                      -- Mô tả khách sạn
);
GO

CREATE TABLE Restaurant (
    restaurant_id INT PRIMARY KEY IDENTITY(1,1),   -- Mã nhà hàng (khóa chính)
    restaurant_name NVARCHAR(100) NOT NULL,        -- Tên nhà hàng
    location NVARCHAR(255),                        -- Địa chỉ nhà hàng
    price_range VARCHAR(50),                       -- Khoảng giá của nhà hàng
    description NVARCHAR(MAX)                      -- Mô tả nhà hàng
);
GO

-- Bảng tour (Tour)
CREATE TABLE Tour (
    tour_id INT PRIMARY KEY IDENTITY(1,1),       -- Mã tour (khóa chính)
    tour_name NVARCHAR(100) NOT NULL,            -- Tên tour
    description NVARCHAR(MAX),                   -- Mô tả tour
    tour_image NVARCHAR(100) NOT NULL,           -- Hình ảnh tour
    price DECIMAL(10, 0) NOT NULL,               -- Giá tour
    duration INT NOT NULL,                        -- Thời lượng tour (số ngày)
    travelby VARCHAR(100) NOT NULL,              -- Du lịch bằng phương tiện
    available_slots INT NOT NULL,                 -- Số chỗ còn trống
    created_at DATETIME DEFAULT GETDATE(),       -- Ngày tạo tour (mặc định hiện tại)
    average_rating FLOAT NULL            -- Điểm đánh giá trung bình (1-5)
);
GO

CREATE TABLE TourWishlist (
    wishlist_id INT PRIMARY KEY IDENTITY(1,1),        -- Mã wishlist (khóa chính)
    wishlist_name NVARCHAR(100) NOT NULL,             -- Tên wishlist
    description NVARCHAR(MAX),                        -- Mô tả wishlist
    price DECIMAL(10, 2) NOT NULL,                    -- Giá wishlist
    hotel_id INT,                                     -- Mã khách sạn (khóa ngoại)
    restaurant_id INT,                                -- Mã nhà hàng (khóa ngoại)
    img_url NVARCHAR(255),                            -- Đường dẫn hình ảnh
    created_at DATETIME NOT NULL,                    -- Ngày tạo wishlist
	FOREIGN KEY (hotel_id) REFERENCES Hotel(hotel_id),             -- Khóa ngoại tham chiếu tới bảng Hotel
    FOREIGN KEY (restaurant_id) REFERENCES Restaurant(restaurant_id)
);
GO

-- Bảng chi tiết tour (Tour_Detail)
CREATE TABLE Tour_Detail (
    tour_detail_id INT PRIMARY KEY IDENTITY(1,1),    -- Mã chi tiết tour (khóa chính)
    tour_id INT,                                     -- Mã tour (khóa ngoại)
    image_url VARCHAR(255),                          -- Đường dẫn hình ảnh
    num_people INT NOT NULL,                         -- Số người đi
    departure_date DATE NOT NULL,                    -- Ngày đi
    return_date DATE NOT NULL,                       -- Ngày về
	daily_activities NVARCHAR(MAX),					-- Mô tả các ngày
    FOREIGN KEY (tour_id) REFERENCES Tour(tour_id) ON DELETE CASCADE  -- Xóa tour thì xóa luôn chi tiết
);
GO

-- Bảng dịch vụ bổ sung cho tour (Tour_Services)
CREATE TABLE Tour_Services (
    service_id INT PRIMARY KEY IDENTITY(1,1),    -- Mã dịch vụ bổ sung (khóa chính)
    service_name NVARCHAR(100) NOT NULL,         -- Tên dịch vụ bổ sung
    price DECIMAL(10, 2) NOT NULL,               -- Giá dịch vụ
    description NVARCHAR(400)                    -- Mô tả dịch vụ
);
GO


-- Bảng Combo Tour
CREATE TABLE TourCombo (
    combo_id INT PRIMARY KEY IDENTITY(1,1),       -- Mã combo tour (khóa chính)
    combo_name NVARCHAR(100) NOT NULL,            -- Tên combo tour
    description NVARCHAR(MAX),                     -- Mô tả chi tiết về combo tour
    price DECIMAL(10, 2) NOT NULL,                -- Giá của combo tour
    hotel_id INT,                                 -- Mã khách sạn (khóa ngoại tham chiếu đến bảng Hotel)
    restaurant_id INT,                            -- Mã nhà hàng (khóa ngoại tham chiếu đến bảng Restaurant)
    img_url NVARCHAR(255),                        -- Đường dẫn ảnh combo tour (URL)
    created_at DATETIME DEFAULT GETDATE(),        -- Ngày tạo combo tour (mặc định hiện tại)
    FOREIGN KEY (hotel_id) REFERENCES Hotel(hotel_id),             -- Khóa ngoại tham chiếu tới bảng Hotel
    FOREIGN KEY (restaurant_id) REFERENCES Restaurant(restaurant_id) -- Khóa ngoại tham chiếu tới bảng Restaurant
);
GO

-- Bảng quan hệ giữa Combo Tour và Dịch vụ bổ sung
CREATE TABLE TourCombo_Services (
    tourcombo_service_id INT PRIMARY KEY IDENTITY(1,1), -- Mã quan hệ (khóa chính)
    combo_id INT,                                         -- ID combo tour
    service_id INT,                                      -- ID dịch vụ
    FOREIGN KEY (combo_id) REFERENCES TourCombo(combo_id),    -- Khóa ngoại tham chiếu tới bảng TourCombo
    FOREIGN KEY (service_id) REFERENCES Tour_Services(service_id) -- Khóa ngoại tham chiếu tới bảng Tour_Services
);
GO

CREATE TABLE Booking (
    booking_id INT PRIMARY KEY IDENTITY(1,1),         -- Mã đặt tour (khóa chính)
    customer_id INT NOT NULL,                          -- Mã khách hàng (khóa ngoại)
    tour_id INT NULL,                                  -- Mã tour (nếu là tour thường)
    combo_id INT NULL,                                 -- Mã combo tour (nếu là combo tour)
    wishlist_id INT,                                   -- Mã wishlist (nếu có)
    staff_id INT,                                      -- Mã nhân viên phụ trách (khóa ngoại)
    booking_date DATE DEFAULT GETDATE(),               -- Ngày đặt tour (mặc định hiện tại)
    total_price DECIMAL(10, 2) NOT NULL,               -- Tổng tiền đặt tour
    num_people INT NOT NULL,                           -- Số lượng người tham gia tour
    booking_status VARCHAR(50) CHECK (booking_status IN ('Pending', 'Confirmed', 'Cancelled')) DEFAULT 'Pending', -- Trạng thái booking
    special_requests TEXT,                             -- Yêu cầu đặc biệt
    departure_date DATE,                               -- Ngày khởi hành
    IsReview TINYINT CHECK (IsReview IN (0, 1)) DEFAULT 0, -- 0: chưa đánh giá, 1: đã đánh giá
    FOREIGN KEY (customer_id) REFERENCES Customer(customer_id),   -- Khóa ngoại tham chiếu bảng Customer
    FOREIGN KEY (tour_id) REFERENCES Tour(tour_id),               -- Khóa ngoại tham chiếu bảng Tour
    FOREIGN KEY (combo_id) REFERENCES TourCombo(combo_id),        -- Khóa ngoại tham chiếu bảng Combo Tour
    FOREIGN KEY (wishlist_id) REFERENCES TourWishlist(wishlist_id), -- Khóa ngoại tham chiếu bảng Wishlist
    FOREIGN KEY (staff_id) REFERENCES Staff(staff_id)             -- Khóa ngoại tham chiếu bảng Staff
);


-- Bảng Invoice 
CREATE TABLE Invoice (
    invoice_id INT PRIMARY KEY IDENTITY(1,1),    -- Mã hóa đơn (khóa chính)
    booking_id INT,                              -- Mã đặt tour (khóa ngoại)
    invoice_date DATETIME DEFAULT GETDATE(),     -- Ngày tạo hóa đơn (mặc định hiện tại)
    total_amount DECIMAL(10, 2) NOT NULL,        -- Tổng tiền hóa đơn
    payment_method VARCHAR(50) CHECK (payment_method IN ('Credit_Card', 'Bank_Transfer', 'E_Wallet', 'Cash')) NOT NULL,  -- Phương thức thanh toán
    payment_status VARCHAR(50) CHECK (payment_status IN ('Pending', 'Completed', 'Failed')) DEFAULT 'Pending',          -- Trạng thái thanh toán
    FOREIGN KEY (booking_id) REFERENCES Booking(booking_id)  -- Khóa ngoại tham chiếu bảng Booking
);
GO


-- Bảng đánh giá tour (Tour_Review)
CREATE TABLE Tour_Review (
    review_id INT PRIMARY KEY IDENTITY(1,1),     -- Mã đánh giá (khóa chính)
    customer_id INT,                             -- Mã khách hàng (khóa ngoại)
    tour_id INT,                                 -- Mã tour (khóa ngoại)
	combo_id INT,
    review_text NVARCHAR(500),                            -- Nội dung đánh giá
    rating INT CHECK (rating >= 1 AND rating <= 5),  -- Điểm đánh giá (1-5)
    review_date DATETIME DEFAULT GETDATE(),      -- Ngày đánh giá (mặc định hiện tại)
    FOREIGN KEY (customer_id) REFERENCES Customer(customer_id), 
    FOREIGN KEY (tour_id) REFERENCES Tour(tour_id),   
    FOREIGN KEY (combo_id) REFERENCES TourCombo(combo_id)             
	
);


GO


CREATE TABLE ImgList (
    img_id INT PRIMARY KEY IDENTITY(1,1),         -- Mã ảnh phụ (khóa chính)
    combo_id INT,                                 -- Mã combo (khóa ngoại tham chiếu đến bảng TourCombo)
	wishlist_id INT,
    img_url NVARCHAR(255) NOT NULL,              -- Đường dẫn ảnh phụ
    FOREIGN KEY (combo_id) REFERENCES TourCombo(combo_id) -- Khóa ngoại tham chiếu tới bảng TourCombo
);
GO

-- Bảng lưu trữ ngày khởi hành của combo tour
CREATE TABLE TourComboDeparture (
    departure_id INT PRIMARY KEY IDENTITY(1,1),     -- Mã ngày khởi hành (khóa chính)
    combo_id INT NOT NULL,                          -- Mã combo tour (khóa ngoại)
    departure_date DATE NOT NULL,                   -- Ngày khởi hành
    price DECIMAL(20, 2) NOT NULL,                  -- Giá tại ngày khởi hành (nếu giá có thể thay đổi)
    available_slots INT NOT NULL,                   -- Số chỗ trống ban đầu
    FOREIGN KEY (combo_id) REFERENCES TourCombo(combo_id) -- Khóa ngoại tham chiếu đến bảng TourCombo
);
GO

CREATE TABLE WishlistDeparture (
    departure_id INT PRIMARY KEY IDENTITY(1,1),       -- Mã ngày khởi hành (khóa chính)
    wishlist_id INT NOT NULL,                         -- Mã wishlist (khóa ngoại)
    departure_date DATE NOT NULL,                     -- Ngày khởi hành
    price DECIMAL(10, 2) NOT NULL,                    -- Giá
    available_slots INT NOT NULL,                     -- Số chỗ còn lại
    FOREIGN KEY (wishlist_id) REFERENCES TourWishlist(wishlist_id)
);
GO

CREATE TABLE WishlistServices (
    tour_wishlist_service_id INT PRIMARY KEY IDENTITY(1,1),
    wishlist_id INT,                         -- Mã wishlist (khóa ngoại)
    service_id INT,                          -- Mã dịch vụ (khóa ngoại)
    FOREIGN KEY (wishlist_id) REFERENCES TourWishlist(wishlist_id),
    FOREIGN KEY (service_id) REFERENCES Tour_Services(service_id)
);


-- Ràng buộc cho bảng Customer
ALTER TABLE Customer
ADD CONSTRAINT CHK_Customer_Email UNIQUE (email);
ALTER TABLE Customer
ADD CONSTRAINT CHK_Customer_Phone_Length CHECK (LEN(phone) >= 10 AND LEN(phone) <= 15);
GO

-- Ràng buộc cho bảng Staff
ALTER TABLE Staff
ADD CONSTRAINT CHK_Staff_Email UNIQUE (email);
ALTER TABLE Staff
ADD CONSTRAINT CHK_Staff_Role CHECK (role IN ('Admin', 'Tour_Manager', 'Staff'));
GO

-- Ràng buộc cho bảng Tour
ALTER TABLE Tour
ADD CONSTRAINT CHK_Tour_Price CHECK (price > 0);
ALTER TABLE Tour
ADD CONSTRAINT CHK_Tour_Duration CHECK (duration > 0);

GO

-- Ràng buộc cho bảng Booking
ALTER TABLE Booking
ADD CONSTRAINT CHK_Booking_Payment_Status CHECK (booking_status IN ('Pending', 'Confirmed', 'Cancelled'));
ALTER TABLE Booking
ADD CONSTRAINT CHK_Booking_Total_Price CHECK (total_price >= 0);
GO

-- Ràng buộc cho bảng Tour_Detail
ALTER TABLE Tour_Detail
ADD CONSTRAINT CHK_Tour_Detail_Num_People CHECK (num_people > 0);
ALTER TABLE Tour_Detail
ADD CONSTRAINT CHK_Tour_Detail_Dates CHECK (departure_date <= return_date);
GO

-- Ràng buộc cho bảng Tour_Review
ALTER TABLE Tour_Review
ADD CONSTRAINT CHK_Tour_Review_Rating CHECK (rating >= 1 AND rating <= 5);
GO

-- Ràng buộc cho mối quan hệ giữa bảng Booking và Tour
ALTER TABLE Booking
ADD CONSTRAINT FK_Booking_Tour FOREIGN KEY (tour_id) REFERENCES Tour(tour_id);
GO

-- Ràng buộc cho mối quan hệ giữa bảng Invoice và Booking
ALTER TABLE Invoice
ADD CONSTRAINT FK_Invoice_Booking FOREIGN KEY (booking_id) REFERENCES Booking(booking_id);
GO

-- Bước 1: Thêm nhân viên vào bảng Staff trực tiếp
INSERT INTO Staff (name, email, phone, role, password)
VALUES 
    ('Pham Trung', 'trung@gmail.com', '0901234567', 'Admin', 'staff@123'), 
    ('Nguyen Van A', 'nguyenvana@example.com', '0901234567', 'Staff', 'staff@123'), 
    ('Tran Thi B', 'tranthib@example.com', '0909876543', 'Tour_Manager', 'tmanager@123');
GO

-- Bước 2: Tạo tài khoản đăng nhập và người dùng cho từng nhân viên

-- Tạo tài khoản đăng nhập và người dùng cho admin
CREATE LOGIN [trung@gmail.com] WITH PASSWORD = 'staff@123';
CREATE USER [trung@gmail.com] FOR LOGIN [trung@gmail.com];
GO

-- Tạo tài khoản đăng nhập và người dùng cho staff
CREATE LOGIN [nguyenvana@example.com] WITH PASSWORD = 'staff@123';
CREATE USER [nguyenvana@example.com] FOR LOGIN [nguyenvana@example.com];
GO

-- Tạo tài khoản đăng nhập và người dùng cho tour manager
CREATE LOGIN [tranthib@example.com] WITH PASSWORD = 'tmanager@123';
CREATE USER [tranthib@example.com] FOR LOGIN [tranthib@example.com];
GO

-- Bước 3: Tạo các role nếu chưa tồn tại
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'AdminRole')
    CREATE ROLE AdminRole;
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'StaffRole')
    CREATE ROLE StaffRole;
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'Tour_Manager')
    CREATE ROLE Tour_Manager;
GO

-- Bước 4: Gán quyền cho các vai trò

-- Phân quyền cho vai trò AdminRole (quyền trên tất cả các bảng trong schema dbo)
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO AdminRole;  -- Quyền trên tất cả các bảng
GRANT EXECUTE ON SCHEMA::dbo TO AdminRole;                         -- Quyền thực thi tất cả các thủ tục
GRANT CONTROL ON DATABASE::DB_TourDuLich TO AdminRole;             -- Quyền kiểm soát toàn bộ database
GO

-- Phân quyền cho vai trò StaffRole
GRANT SELECT, INSERT, UPDATE ON Tour TO StaffRole;
GRANT SELECT ON Staff TO StaffRole;
GRANT SELECT, INSERT, UPDATE, DELETE ON Booking TO StaffRole;
GRANT SELECT ON Tour_Review TO StaffRole;
GO

-- Phân quyền cho vai trò Tour_Manager
GRANT SELECT, INSERT, UPDATE ON Tour TO Tour_Manager;
GRANT SELECT ON Booking TO Tour_Manager;
GRANT INSERT, UPDATE ON Booking TO Tour_Manager;
GRANT SELECT ON Tour_Review TO Tour_Manager;
GO

-- Bước 5: Gán người dùng vào vai trò

-- Gán admin vào vai trò AdminRole
EXEC sp_addrolemember 'AdminRole', 'trung@gmail.com';
GO

-- Gán staff vào vai trò StaffRole
EXEC sp_addrolemember 'StaffRole', 'nguyenvana@example.com';
GO

-- Gán tour manager vào vai trò Tour_Manager
EXEC sp_addrolemember 'Tour_Manager', 'tranthib@example.com';
GO

-- Bước 6: Kiểm tra quyền của người dùng (tuỳ chọn)

-- Kiểm tra quyền của Admin
EXECUTE AS USER = 'trung@gmail.com';
SELECT * FROM fn_my_permissions(NULL, 'DATABASE');
REVERT;
GO

-- Kiểm tra quyền của Staff
EXECUTE AS USER = 'nguyenvana@example.com';
SELECT * FROM fn_my_permissions(NULL, 'DATABASE');
REVERT;
GO

-- Kiểm tra quyền của Tour Manager
EXECUTE AS USER = 'tranthib@example.com';
SELECT * FROM fn_my_permissions(NULL, 'DATABASE');
REVERT;
GO


-- Nhập liệu cho bảng Customer
INSERT INTO Customer (name, email, phone,username, password, address, dob)
VALUES
('Nguyen Van A', 'nguyenvana@example.com', '0901234567','VanA01', '202cb962ac59075b964b07152d234b70', '123 Le Loi, HCM', '1990-01-01'),
('Tran Thi B', 'tranthib@example.com', '0912345678','ThiB02', '202cb962ac59075b964b07152d234b70', '456 Nguyen Trai, HCM', '1992-02-15'),
('Le Van C', 'levanc@example.com', '0923456789','VanC03', '202cb962ac59075b964b07152d234b70', '789 Hai Ba Trung, HN', '1985-03-20'),
('Pham Trung', 'tenyyyy495@gmail.com', '0934567890','Trung', '202cb962ac59075b964b07152d234b70', '12 Ba Trieu, HN', '1995-04-10'),
('Do Van E', 'dovane@example.com', '0945678901','TrungAnh', '202cb962ac59075b964b07152d234b70', '45 Tran Hung Dao, HCM', '2000-05-05');
GO

-- Nhập liệu cho bảng Hotel (Khách sạn tại Đà Lạt)
INSERT INTO Hotel (hotel_name, price_per_night, location, description) 
VALUES 
('Edensee Lake Resort & Spa', 1760000, 'Hồ Tuyền Lâm, Đà Lạt', 'Khách sạn 5 sao với phong cách Pháp sang trọng bên hồ Tuyền Lâm, Đà Lạt. Resort cao cấp với không gian yên tĩnh, dịch vụ đẳng cấp và không gian đẹp, thích hợp cho kỳ nghỉ thư giãn.'),
('Ana Villas Đà Lạt Resort & Spa', 2150000, 'Lê Lai, Phường 5, Đà Lạt', 'Khu nghỉ dưỡng với kiến trúc cổ điển Pháp, nằm giữa rừng thông xanh mát. Ana Villas là điểm đến lý tưởng cho những ai muốn tận hưởng không khí trong lành và không gian yên tĩnh.'),
('Dalat Palace Luxury Hotel', 4500000, '2 Đường Trần Phú, Đà Lạt', 'Khách sạn 5 sao với lịch sử lâu đời, tọa lạc ngay trung tâm Đà Lạt. Kiến trúc cổ điển châu Âu và dịch vụ đẳng cấp quốc tế. Nơi lý tưởng cho các kỳ nghỉ sang trọng.'),
('Bich Dao Hotel', 800000, '42 Đường 3 Tháng 4, Đà Lạt', 'Khách sạn giá rẻ nhưng chất lượng tốt, nằm gần trung tâm Đà Lạt. Bich Dao Hotel cung cấp các phòng nghỉ thoải mái và dịch vụ chu đáo với giá cả phải chăng.'),
('Ladalat Hotel', 1800000, '5 Đường Hùng Vương, Đà Lạt', 'Khách sạn 4 sao nằm tại trung tâm thành phố Đà Lạt, với thiết kế hiện đại, phòng nghỉ tiện nghi và dịch vụ chất lượng. Là điểm dừng chân lý tưởng cho du khách khi đến thăm Đà Lạt.'),
('Swiss-BelResort Tuyền Lâm', 2500000, 'Khu du lịch Hồ Tuyền Lâm, Đà Lạt', 'Resort 5 sao sang trọng, nằm gần hồ Tuyền Lâm, với không gian yên tĩnh, dịch vụ chăm sóc khách hàng tuyệt vời. Thích hợp cho những kỳ nghỉ dưỡng dài ngày.'),
('TTC Hotel Premium – Ngọc Lan', 2000000, '42 Nguyễn Chí Thanh, Đà Lạt', 'Khách sạn 4 sao nằm gần hồ Xuân Hương, với thiết kế thanh lịch, dịch vụ tiện nghi, phù hợp cho cả khách công tác và khách du lịch.'),
('Novotel Dalat', 3200000, 'Đường 3 Tháng 4, Đà Lạt', 'Khách sạn 4 sao hiện đại với bể bơi, spa, nhà hàng cao cấp. Nằm gần trung tâm Đà Lạt, Novotel là nơi nghỉ dưỡng tuyệt vời cho các du khách.'),
('Mimosa Resort & Spa', 2000000, 'Thung Lũng Tình Yêu, Đà Lạt', 'Resort sang trọng với không gian xanh mát, nằm gần khu du lịch Thung Lũng Tình Yêu. Các dịch vụ spa và nhà hàng tại Mimosa Resort sẽ mang lại sự thư giãn tuyệt đối cho du khách.'),
('The Lux Hotel', 1500000, '10 Phan Chu Trinh, Đà Lạt', 'Khách sạn 3 sao với thiết kế hiện đại và dịch vụ tiện nghi, chỉ cách trung tâm Đà Lạt vài phút đi bộ. The Lux Hotel là lựa chọn hoàn hảo cho những ai muốn khám phá thành phố Đà Lạt.');  
GO


-- Nhập liệu cho bảng Restaurant (Nhà hàng tại Đà Lạt)
INSERT INTO Restaurant (restaurant_name, location, price_range, description)
VALUES 
('Nhà hàng Edensee', 'Hồ Tuyền Lâm, Đà Lạt', '500,000 - 1,000,000 VND', 'Nhà hàng sang trọng với các món ăn Âu Á, phục vụ trong không gian ấm cúng bên hồ Tuyền Lâm. Đây là địa điểm lý tưởng cho những bữa tối lãng mạn hoặc các buổi gặp gỡ doanh nhân.'),
('Nhà hàng Ana Villas', 'Lê Lai, Phường 5, Đà Lạt', '300,000 - 700,000 VND', 'Nhà hàng với không gian ấm cúng giữa rừng thông, chuyên phục vụ các món ăn Âu – Việt kết hợp, mang lại trải nghiệm ẩm thực độc đáo cho thực khách.'),
('Nhà hàng Le Chalet Dalat', '17C Đường Trần Hưng Đạo, Đà Lạt', '400,000 - 800,000 VND', 'Nhà hàng Pháp mang đậm dấu ấn châu Âu với các món ăn Pháp truyền thống và các loại rượu vang hảo hạng, mang đến một trải nghiệm ẩm thực đẳng cấp tại Đà Lạt.'),
('Nhà hàng 3M', '29 Trần Phú, Đà Lạt', '150,000 - 300,000 VND', 'Nhà hàng nổi tiếng với các món ăn Việt Nam truyền thống, đặc biệt là các món lẩu và nướng. Không gian rộng rãi, thích hợp cho các buổi tụ họp bạn bè hoặc gia đình.'),
('Nhà hàng Mê Linh', 'Thôn Mê Linh, Đà Lạt', '200,000 - 500,000 VND', 'Nhà hàng nổi bật với các món ăn đặc sản Đà Lạt và đặc biệt là cà phê Mê Linh nổi tiếng, mang lại cảm giác thư giãn giữa không gian xanh mát của Đà Lạt.'),
('Nhà hàng Vườn Yên', 'Phường 9, Đà Lạt', '300,000 - 600,000 VND', 'Nhà hàng phục vụ các món ăn đặc sản Đà Lạt như rau, củ quả hữu cơ trồng tại vườn, đặc biệt các món ăn chay rất được ưa chuộng tại đây.'),
('Nhà hàng Ngọc Lan', '42 Nguyễn Chí Thanh, Đà Lạt', '350,000 - 600,000 VND', 'Nhà hàng nổi tiếng với các món Âu, đặc biệt là các món khai vị và món chính phong phú, cùng với không gian lịch sự và sang trọng.'),
('Nhà hàng Hương Sơn', 'Tổ 6, Phường 5, Đà Lạt', '150,000 - 400,000 VND', 'Nhà hàng chuyên các món ăn Việt Nam dân dã với hương vị đặc trưng, rất thích hợp cho những bữa cơm gia đình.'),
('Nhà hàng Vạn Thạnh', '21 Hùng Vương, Đà Lạt', '100,000 - 250,000 VND', 'Nhà hàng với thực đơn đa dạng từ các món ăn truyền thống đến các món ăn nhanh, thích hợp cho nhóm bạn trẻ và gia đình.'),
('Nhà hàng Xuân An', 'Đường Bùi Thị Xuân, Đà Lạt', '200,000 - 500,000 VND', 'Nhà hàng chuyên các món ăn đặc sản Đà Lạt, với phong cách trang trí mộc mạc và thân thiện, phù hợp cho các gia đình và du khách tìm kiếm sự giản dị.');  
GO


-- Nhập liệu cho bảng Tour
INSERT INTO Tour (tour_name, description, tour_image, price, duration, travelby,available_slots)
VALUES
(N'Tour Da Lat 1', N'Tham quan Hồ Xuân Hương và Thác Datanla', 'pack1.jpg', 5000000, 3, 'Xe khách', 18),
(N'Tour Da Lat 2', N'Tham quan Vườn Hoa Thành Phố và Đồi Chè Cầu Đất', 'pack2.jpg', 7000000, 4, 'Máy bay', 18),
(N'Tour Da Lat 3', N'Trekking tại Núi Langbiang và tham quan Làng Cù Lần', 'pack3.jpg', 6000000, 5, 'Xe khách', 18),
(N'Tour Da Lat 4', N'Khám phá KDL Thung Lũng Tình Yêu và Đà Lạt Night Market', 'pack4.jpg', 8000000, 3, 'Xe khách', 18),
(N'Tour Da Lat 5', N'Tour trải nghiệm thiên nhiên tại Vườn Quốc Gia Bidoup - Núi Bà', 'pack5.jpg', 4500000, 2, 'Xe khách', 18),
(N'Tour Đà Lạt 6', N'Tham quan Ga Đà Lạt và Chùa Linh Phước', 'pack6.jpg', 5500000, 3, 'Xe khách', 20),
(N'Tour Đà Lạt 7', N'Trekking núi Voi và tham quan Làng Hoa Vạn Thành', 'pack7.jpg', 6500000, 4, 'Xe khách', 18),
(N'Tour Đà Lạt 8', N'Khám phá Hồ Tuyền Lâm và Thiền Viện Trúc Lâm', 'pack8.jpg', 7000000, 3, 'Xe máy', 12),
(N'Tour Đà Lạt 9', N'Tham quan Nhà thờ Con Gà và Biệt thự Hằng Nga', 'pack9.jpg', 5000000, 2, 'Xe máy', 15),
(N'Tour Đà Lạt 10', N'Trekking thác Pongour và KDL Đường hầm Đất Sét', 'pack10.jpg', 6000000, 3, 'Xe khách', 20),
(N'Tour Đà Lạt 11', N'Khám phá Vườn Quốc Gia Bidoup và Thác Prenn', 'pack11.jpg', 7500000, 4, 'Xe khách', 10),
(N'Tour Đà Lạt 12', N'Tham quan Làng Hoa Thái Phiên và Thung Lũng Vàng', 'pack12.jpg', 4000000, 2, 'Xe máy', 25),
(N'Tour Đà Lạt 13', N'Tham quan Hồ Than Thở và Trại Mát', 'pack13.jpg', 4800000, 3, 'Xe khách', 20),
(N'Tour Đà Lạt 14', N'Trekking đồi Thiên Phúc Đức và thăm đồi chè Cầu Đất', 'pack14.jpg', 6500000, 4, 'Xe khách', 12),
(N'Tour Đà Lạt 15', N'Khám phá Khu du lịch Lạc Dương và tham quan Hồ Suối Vàng', 'pack15.jpg', 7200000, 5, 'Xe khách', 8);


-- Nhập liệu cho bảng TourCombo
INSERT INTO TourCombo (combo_name, description, price, hotel_id, restaurant_id, img_url)
VALUES 
(N'Combo nghỉ dưỡng Edensee', 
 N'Tọa lạc bên hồ Tuyền Lâm thơ mộng, Đà Lạt Edensee Lake Resort & Spa được ví như một thiên đường nghỉ dưỡng trên núi. Khu nghỉ dưỡng có 112 phòng nghỉ được thiết kế theo phong cách Pháp thời kỳ Đông Dương đặc trưng, sang trọng mà ấm áp. Mỗi phòng nghỉ được trang bị các tiện nghi hiện đại chuẩn 5 sao quốc tế. 
 Đà Lạt Edensee Lake Resort & Spa còn có phương tiện giải trí tuyệt vời như spa, mát xa, bể bơi, phòng thể dục, vườn để làm cho chuyến đi của bạn thật khó quên. Nếu bạn cần một nơi nghỉ dưỡng sang trọng mà tiện nghi, trở về với thiên nhiên trong lành ở Đà Lạt, hãy đến Đà Lạt Edensee Lake Resort & Spa.',
 4500000, 1, 1, 'combo1.jpg'),

(N'Combo khám phá Ana Villas', 
 N'Khu nghỉ mát Ana Villas Đà Lạt tọa lạc giữa không khí mát lạnh của cao nguyên Đà Lạt, ẩn mình trong một không gian xanh trải dài của những ngọn đồi nhấp nhô giữa rừng thông, xen kẽ khu vườn xinh xắn tạo nên không gian yên tĩnh và lãng mạn. Mang nét đặc trưng từ thiết kế đến vẻ quyến rũ vốn có của kiến trúc Pháp từ những năm 1920 – 1930 được tô điểm thêm tầm nhìn bao quát toàn bộ khung cảnh thành phố và những vườn rau xanh mát dọc theo lưng đồi. 
 Mỗi loại phòng trong mỗi villa mang đến cảm giác khác nhau với thiết kế độc đáo, từ căn phòng nằm ở tầng áp mái bằng gỗ kiểu cổ có tầm nhìn hướng ra khung cảnh dãy núi phía xa tới thiết kế với lò sưởi và cánh cửa gỗ truyền thống dẫn thẳng ra khu vườn bao quanh.',  
 3500000, 1, 1, 'combo2.jpg'),

(N'PALACE HERITAGE 5*', 
 N'Được xây dựng từ năm 1922, khách sạn Đà Lạt Palace Heritage thể hiện đẳng cấp thanh lịch sang trọng với dịch vụ hoàn hảo và vẻ duyên dáng thời thuộc địa Pháp. Nhìn xuống hồ Xuân Hương tráng lệ của Đà Lạt, khách sạn Palace Heritage tọa lạc giữa sân vườn rộng năm héc-ta bao phủ bởi những hàng thông xanh trãi dài, với chuỗi phòng họp dành cho các sự kiện đặc biệt. Khi nói về ẩm thực cao cấp, nhà hàng của chúng tôi đáp ứng mọi thị hiếu. 43 Phòng và Phòng Suite rộng rãi và được thiết kế theo phong cách Edwardian và Art Deco tiêu biểu. Nhìn ra hướng hồ hoặc nhà thờ, tiện nghi cao cấp và dịch vụ phong phú độc đáo tại khách sạn Palace Heritage sẽ làm hài lòng quý khách.',
 8000000, 2, 1, 'combo3.jpg'),

(N'KHÁCH SẠN LA ĐÀ LẠT 5*',
 N'Khách sạn được xây dựng theo tiêu chuẩn 5 sao, không gian khép kín, tiện nghi cho một chuyến nghỉ dưỡng cao cấp. Thiết kế sang trọng với kiến trúc bán cổ điển, cùng các khu tiện ích khác như hồ bơi, phòng gym, phòng karaoke, sân tennis, khu vui chơi, bãi đỗ xe. Không gian các phòng lưu trú ở đây nổi trội khi được kết hợp giữa nghệ thuật xây dựng và khung cảnh thiên nhiên vốn có của cao nguyên Đà Lạt. Theo đó, tất cả các phòng đều có ban công lớn. Đứng từ ban công, du khách có thể nhìn ngắm vẻ đẹp mộng mơ của Đà Lạt, Thung lũng Tình Yêu và hồ Đa Thiện.',
 7500000, 2, 2, 'combo4.jpg'),

(N'RIVER PRINCE 4*',
 N'Tọa lạc ở vị trí đẹp của Trung tâm Thành phố Đà Lạt, khách sạn River Prince hưởng được rất nhiều lợi thế trong ngắm cảnh, lãng mạn, vui chơi gia đình. Tại khách sạn River Prince, dịch vụ hoàn hảo và thiết bị tối tân tạo nên một kì nghỉ khó quên. Những tính năng hàng đầu của khách sạn bao gồm dịch vụ giặt là, đưa đón khách sạn/sân bay, thủ tục nhận phòng/trả phòng nhanh, dịch vụ Internet, phòng gia đình. Khách sạn River Prince có 104 phòng, tất cả đồ nội thất được thiết kế hài hòa mang lại cảm giác gần gũi, dễ chịu. Bên cạnh đó, khách sạn còn gợi ý cho bạn những hoạt động vui chơi giải trí bảo đảm bạn luôn thấy hứng thú trong suốt kì nghỉ.',
 6000000, 1, 2, 'combo5.jpg'),

(N'TERRACOTTA HOTEL & RESORT 4*',
 N'Nằm ở một vị trí đắc địa, bao phủ bởi rừng thông xanh, nhìn xuống bờ hồ Tuyền Lâm êm đềm trong vắt, Terracotta Hotel & Resort Đà Lạt ẩn hiện trong màn sương mờ ảo của buổi sớm mai như một phân khu nghỉ dưỡng đẳng cấp, sang trọng. Được thiết kế hài hòa với thiên nhiên, được che phủ bên dưới những tán lá thông xanh mướt tạo nên một công trình kiến trúc khác biệt. Khu nghỉ dưỡng Terracotta Đà Lạt nằm gọn trong bán đảo rộng hơn 17 hécta gồm 240 phòng nghỉ và 21 căn biệt thự ven hồ, được trang bị đầy đủ tiện nghi đạt tiêu chuẩn 4 sao. Là một trong những điểm đến lý tưởng dành cho những ai yêu thích thiên nhiên và mong muốn tìm đến một nơi nghỉ dưỡng trong lành, thư giãn đúng nghĩa.',
 6500000, 1, 1, 'combo6.jpg'),

(N'SWISS-BELRESORT TUYỀN LÂM 4*',
 N'Được bao quanh bởi các ngọn đồi, Swiss-Belresort Tuyền Lâm Đà Lạt sở hữu một cảnh quan đặc biệt và một không khí trong lành. Nhờ được thiết kế như một khu vườn xanh, khách sạn 5 sao Đà Lạt này được bao bọc trong một môi trường thiên nhiên tuyệt vời. Với kiến trúc Anglo-Normand theo phong cách vùng miền quê Anh - Pháp, nằm gọn giữa một sân gôn 18 lỗ, đã tạo nên điểm nhấn để khách có thể nhìn ngắm trong khi đi dạo quanh các đường gôn. Khách sạn được thiết kế hài hòa nhưng đầy tinh tế và quyến rũ. Swiss-Café là nhà hàng theo thiết kế đương đại với phòng cách ẩm thực hiện đại, là một nơi lý tưởng cho một bữa ăn nhanh hay một bữa ăn hoàn hảo. Nhà hàng phục vụ các món ăn Âu và Á.',
 7000000, 2, 2, 'combo7.jpg'),

(N'BEST WESTERN PLAZA 3*',
 N'Khách sạn Best Western Plaza Đà Lạt tọa lạc tại vị trí lý tưởng, nằm ngay trung tâm thành phố. Khách sạn ngay bên cạnh hồ Xuân Hương, cách chợ Đà Lạt khoảng 5 phút đi bộ, cách sân bay Liên Khương 30km. Khách sạn được thiết kế theo kiến trúc Châu Âu hòa hợp với lối trang trí hiện đại theo tiêu chuẩn Quốc tế 3 sao, với các cửa sổ riêng biệt nhìn ra trung tâm thành phố, công viên và Hồ Xuân Hương. Mỗi phòng đều được trang bị phòng tắm vòi sen, bồn tắm và toilet riêng biệt, điện thoại quốc tế, mini-bar, truyền hình cáp, két sắt an toàn. Ngoài ra khách sạn còn cung cấp các dịch vụ phòng ở, nhà hàng, massage, karaoke, phòng tập thể dục, dịch vụ văn phòng, quầy lưu niệm.',
 5000000, 2, 1, 'combo8.jpg');

-- Dữ liệu cho bảng Booking 
INSERT INTO Booking (customer_id, tour_id, combo_id, staff_id, total_price, num_people, booking_status, special_requests, booking_date,departure_date)
VALUES
(4, NULL, 1, 3, 6400000, 5, 'Cancelled', 'Changed travel dates', '2024-9-20','2024-12-19'),   -- Combo tour
(1, 1, NULL, 2, 15000000, 3, 'Confirmed', 'No special requests', '2024-01-15','2024-12-30'),  -- Tour thường
(2, 2, NULL, 2, 14000000, 2, 'Pending', 'Need vegetarian meals', '2024-02-05','2024-12-11'),  -- Tour thường
(3, 3, NULL, 1, 12000000, 4, 'Confirmed', 'Want private room', '2024-03-20','2024-12-20'),    -- Tour thường
(5, NULL, 2, 2, 13500000, 3, 'Confirmed', 'Require baby seat', '2024-04-10','2024-10-20'),     -- Combo tour
(5, 1, NULL, 1, 5000000, 1, 'Confirmed', 'No special requests', '2024-04-11','2024-10-20'),    -- Tour thường
(4, NULL, 2, 2, 14000000, 2, 'Confirmed', 'No special requests', '2024-05-01','2024-10-20'),   -- Combo tour
(3, 3, NULL, 1, 18000000, 3, 'Confirmed', 'Require special meals', '2024-05-16','2024-10-20'), -- Tour thường
(2, NULL, 2, 2, 8000000, 1, 'Pending', 'Looking for more information', '2024-06-05','2024-10-20'), -- Combo tour
(1, 5, NULL, 2, 13500000, 4, 'Cancelled', 'Changed travel dates', '2024-06-12','2024-10-20'),  -- Tour thường
(1, NULL, 1, 3, 5500000, 2, 'Confirmed', 'No special requests', '2024-07-21','2024-10-20'),    -- Combo tour
(2, 7, NULL, 2, 6500000, 2, 'Confirmed', 'Need extra luggage space', '2024-08-16','2024-10-20'), -- Tour thường
(3, NULL, 1, 2, 7000000, 3, 'Confirmed', 'Require baby seat', '2024-09-11','2024-10-20'),      -- Combo tour
(4, 9, NULL, 3, 4000000, 2, 'Pending', 'Request for group discount', '2024-10-02','2024-10-20'), -- Tour thường
(5, NULL, 1, 2, 6000000, 5, 'Confirmed', 'No special requests', '2024-11-21','2024-10-20'),
(2, 4, NULL, 1, 20000000, 3, 'Confirmed', 'Need early check-in', '2024-06-01', '2024-12-01'),  
(1, 3, NULL, 2, 12000000, 2, 'Pending', 'Looking for a discount', '2024-07-05', '2024-10-15'),  
(4, 5, NULL, 3, 10000000, 4, 'Confirmed', 'Want to visit other cities', '2024-08-01', '2024-10-05'),  
(5, NULL, 3, 1, 7000000, 2, 'Confirmed', 'Want to change departure date', '2024-09-15', '2024-11-15'),  
(3, NULL, 2, 2, 15000000, 5, 'Pending', 'Need VIP seats', '2024-09-20', '2024-12-25'),  
(2, NULL, 1, 1, 5000000, 1, 'Confirmed', 'Looking for group discounts', '2024-10-01', '2024-12-15'),  
(5, 4, NULL, 1, 8000000, 2, 'Confirmed', 'Need extra luggage', '2024-10-05', '2024-12-01'),  
(1, 6, NULL, 2, 11000000, 3, 'Confirmed', 'Need airport pick-up', '2024-10-10', '2024-12-20'),  
(4, NULL, 1, 3, 6500000, 2, 'Pending', 'Looking for vegetarian options', '2024-10-15', '2024-12-20'),  
(2, 8, NULL, 1, 10500000, 3, 'Confirmed', 'Special meal requests', '2024-10-25', '2024-12-10'),  
(3, NULL, 3, 2, 9000000, 4, 'Confirmed', 'Want private room', '2024-11-01', '2024-12-01'),  
(5, 9, NULL, 1, 13000000, 2, 'Pending', 'Need a bigger room', '2024-11-15', '2024-12-05'),  
(4, NULL, 4, 2, 10500000, 3, 'Confirmed', 'Looking for group discount', '2024-12-01', '2024-12-15'),  
(2, NULL, 5, 1, 8500000, 4, 'Confirmed', 'Want early check-in', '2024-12-05', '2024-12-20'),  
(3, 6, NULL, 2, 11000000, 2, 'Pending', 'Need shuttle service', '2024-12-10', '2024-12-25'),  
(1, 7, NULL, 3, 11500000, 3, 'Confirmed', 'Need more luggage space', '2024-12-15', '2024-12-25'),  
(4, 8, NULL, 1, 12000000, 2, 'Pending', 'Need extra meals', '2024-12-20', '2024-12-30'),  
(2, NULL, 6, 3, 10000000, 4, 'Confirmed', 'Want free cancellation', '2024-12-25', '2024-12-31'),  
(5, 7, NULL, 1, 11000000, 5, 'Confirmed', 'Looking for honeymoon package', '2024-12-30', '2024-12-31'),  
(3, 5, NULL, 2, 9500000, 3, 'Confirmed', 'Want flexible dates', '2024-12-31', '2025-01-10'),  
(1, NULL, 1, 3, 10500000, 2, 'Confirmed', 'Looking for extra activities', '2025-01-05', '2025-01-20'),  
(4, 4, NULL, 1, 9500000, 3, 'Confirmed', 'Want to visit nearby attractions', '2025-01-10', '2025-02-10');  
go

-- Dữ liệu cho bảng Invoice
INSERT INTO Invoice (booking_id, invoice_date, total_amount, payment_method, payment_status)
VALUES 
    (1, '2024-01-16', 16500000.00, 'Credit_Card', 'Completed'),    
    (2, '2024-01-21', 10200000.00, 'Cash', 'Completed'),            
    (3, '2024-02-11', 25000000.00, 'Bank_Transfer', 'Completed'), 
    (4, '2024-02-16', 11000000.00, 'E_Wallet', 'Pending'),         
    (5, '2024-03-06', 18100000.00, 'Credit_Card', 'Completed'),     
    (6, '2024-03-12', 30100000.00, 'Cash', 'Failed'),              
    (7, '2024-04-02', 22020000.00, 'E_Wallet', 'Completed'),        
    (8, '2024-04-17', 17080000.00, 'Bank_Transfer', 'Pending'),      
    (9, '2024-05-05', 25900000.00, 'Credit_Card', 'Completed'),    
    (10, '2024-05-11', 29700000.00, 'Cash', 'Completed'),          
    (11, '2024-05-15', 21800000.00, 'Bank_Transfer', 'Completed'), 
    (12, '2024-05-25', 1500000.00, 'E_Wallet', 'Pending'),           
    (13, '2024-06-05', 2600000.00, 'Credit_Card', 'Failed'),      
    (14, '2024-06-30', 2800000.00, 'Cash', 'Completed'),           
    (15, '2024-07-15', 3000000.00, 'E_Wallet', 'Completed'), 
    (16, '2024-07-20', 32000000.00, 'Credit_Card', 'Completed'),
    (17, '2024-08-01', 13500000.00, 'Cash', 'Pending'),
    (18, '2024-08-10', 17500000.00, 'Bank_Transfer', 'Completed'),
    (19, '2024-08-18', 21000000.00, 'E_Wallet', 'Completed'),
    (20, '2024-09-02', 12500000.00, 'Credit_Card', 'Failed'),
    (21, '2024-09-05', 4500000.00, 'Cash', 'Completed'),
    (22, '2024-09-15', 35000000.00, 'Bank_Transfer', 'Pending'),
    (23, '2024-09-25', 27000000.00, 'E_Wallet', 'Completed'),
    (24, '2024-10-01', 19800000.00, 'Credit_Card', 'Completed'),
    (25, '2024-10-07', 17500000.00, 'Cash', 'Pending'),
    (26, '2024-10-15', 32000000.00, 'Bank_Transfer', 'Failed'),
    (27, '2024-10-25', 24000000.00, 'E_Wallet', 'Completed'),
    (28, '2024-11-05', 22500000.00, 'Credit_Card', 'Completed'),
    (29, '2024-11-10', 15000000.00, 'Cash', 'Completed'),
    (30, '2024-11-15', 13000000.00, 'Bank_Transfer', 'Completed'),
    (31, '2024-11-25', 18500000.00, 'E_Wallet', 'Pending'),
    (32, '2024-12-02', 31000000.00, 'Credit_Card', 'Failed'),
    (33, '2024-12-10', 27000000.00, 'Cash', 'Completed'),
    (34, '2024-12-20', 21000000.00, 'Bank_Transfer', 'Completed'),
    (35, '2024-12-25', 22000000.00, 'E_Wallet', 'Completed'); 
GO
        
      
   
      

-- Nhập liệu cho bảng Tour_Detail
INSERT INTO Tour_Detail (tour_id, image_url, num_people, departure_date, return_date, daily_activities)
VALUES
(1, 'dl1.jpg', 15, '2024-12-01', '2024-12-03', N'Day 1: Tham quan Hồ Xuân Hương; Day 2: Thác Datanla và trải nghiệm cảm giác mạnh với trò chơi mạo hiểm.'),
(2, 'dl2.jpg', 12, '2024-12-05', '2024-12-08', N'Day 1: Tham quan Vườn Hoa Thành Phố; Day 2: Khám phá Đồi Chè Cầu Đất và thưởng thức trà.'),
(3, 'dl3.jpg', 20, '2024-11-15', '2024-11-19', N'Day 1: Trekking tại Núi Langbiang; Day 2: Tham quan Làng Cù Lần và trải nghiệm văn hóa địa phương.'),
(4, 'dl4.jpg', 8, '2024-10-20', '2024-10-22', N'Day 1: Khám phá Thung Lũng Tình Yêu; Day 2: Tham quan Đà Lạt Night Market và thưởng thức đặc sản.'),
(5, 'dl5.jpg', 25, '2024-11-25', '2024-11-26', N'Day 1: Trekking tại Vườn Quốc Gia Bidoup; Day 2: Trải nghiệm cắm trại giữa thiên nhiên và đêm lửa trại.'),
(6, 'dl6.jpg', 18, '2024-12-10', '2024-12-12', N'Day 1: Tham quan Ga Đà Lạt; Day 2: Chùa Linh Phước.'),
(7, 'dl7.jpg', 16, '2024-11-18', '2024-11-21', N'Day 1: Trekking núi Voi; Day 2: Làng Hoa Vạn Thành.'),
(8, 'dl8.jpg', 10, '2024-12-15', '2024-12-17', N'Day 1: Hồ Tuyền Lâm; Day 2: Thiền Viện Trúc Lâm.'),
(9, 'dl9.jpg', 13, '2024-12-05', '2024-12-06', N'Day 1: Nhà thờ Con Gà; Day 2: Biệt thự Hằng Nga.'),
(10, 'dl10.jpg', 18, '2024-11-10', '2024-11-12', N'Day 1: Trekking thác Pongour; Day 2: Đường hầm Đất Sét.'),
(11, 'dl11.jpg', 8, '2024-11-22', '2024-11-25', N'Day 1: Vườn Quốc Gia Bidoup; Day 2: Thác Prenn.'),
(12, 'dl12.jpg', 20, '2024-12-01', '2024-12-02', N'Day 1: Làng Hoa Thái Phiên; Day 2: Thung Lũng Vàng.'),
(13, 'dl13.jpg', 18, '2024-12-05', '2024-12-07', N'Day 1: Hồ Than Thở; Day 2: Trại Mát.'),
(14, 'dl14.jpg', 10, '2024-11-28', '2024-12-01', N'Day 1: Trekking đồi Thiên Phúc Đức; Day 2: Đồi chè Cầu Đất.'),
(15, 'dl15.jpg', 6, '2024-12-20', '2024-12-24', N'Day 1: Khu du lịch Lạc Dương; Day 2: Hồ Suối Vàng.');

GO


GO

-- Nhập liệu cho bảng Tour_Review
INSERT INTO Tour_Review (customer_id, tour_id, review_text, rating)
VALUES
(1, 1, N'Kinh nghiệm , tour rất đẹp.',5),
(1, 1, N'Kinh nghiệm , tour rất đẹp.', 4),
(1, 2, N'Kinh nghiệm tuyệt vời, tour rất đẹp.', 5),
(2, 2, N'Chuyến đi rất hay, nhưng thời tiết không tốt lắm.', 4),
(3, 3, N'Yêu thích trải nghiệm trekking ở đây!', 5),
(4, 4, N'Không đáng giá cho giá tiền, dịch vụ trung bình.', 3),
(5, 5, N'Bãi biển đẹp, tôi sẽ quay lại.', 4);
GO


GO

-- Nhập liệu cho bảng Tour_Services (Dịch vụ Tour tại Đà Lạt)
INSERT INTO Tour_Services (service_name, price, description)
VALUES
(N'Đón tại sân bay', 500000, N'Dịch vụ đón khách từ sân bay Liên Khương về khách sạn tại Đà Lạt.'),
(N'Gói ăn uống', 1000000, N'Bao gồm bữa sáng và bữa trưa trong suốt chuyến đi tại Đà Lạt.'),
(N'Hướng dẫn viên riêng', 1500000, N'Dịch vụ hướng dẫn viên tour riêng suốt hành trình tham quan Đà Lạt.'),
(N'Thiết bị cắm trại', 300000, N'Trang bị đầy đủ thiết bị cắm trại cho các chuyến du lịch dã ngoại tại Đà Lạt.'),
(N'Trải nghiệm VIP', 2000000, N'Trải nghiệm VIP dành riêng cho tour du lịch Đà Lạt với các dịch vụ cao cấp.'),
(N'Xe riêng', 1200000, N'Dịch vụ cho thuê xe riêng để di chuyển linh hoạt tại Đà Lạt trong suốt chuyến đi.'),
(N'Chụp ảnh chuyên nghiệp', 800000, N'Dịch vụ chụp ảnh chuyên nghiệp tại các địa điểm đẹp của Đà Lạt.'),
(N'Gói spa & thư giãn', 1500000, N'Dịch vụ spa thư giãn và massage tại các resort, khách sạn cao cấp ở Đà Lạt.'),
(N'Tour xe đạp', 400000, N'Dịch vụ cho thuê xe đạp và tour khám phá Đà Lạt theo phong cách mới.'),
(N'Bữa tối lãng mạn', 1000000, N'Bữa tối lãng mạn với cảnh quan đẹp tại các nhà hàng đặc biệt ở Đà Lạt.');  
GO

INSERT INTO TourCombo_Services (combo_id, service_id)
VALUES 
(1, 1),  
(1, 2), 
(1, 5), 
(2, 2), 
(2, 3),  
(3, 4),  
(3, 2), 
(4, 3),  
(4, 6),  
(5, 7), 
(5, 8), 
(6, 2),
(6, 4),  
(6, 9);

GO
-- Nhập liệu cho bảng Booking_Services
INSERT INTO ImgList (combo_id , img_url )
VALUES
(1, 'cb1.jpg'),
(1, 'cb2.jpg'),
(1, 'cb3.jpg'),
(1, 'cb4.jpg'),
(1, 'cb5.jpg');


GO

INSERT INTO TourWishlist (
    wishlist_name, description, price, hotel_id, restaurant_id, img_url, created_at
) VALUES 
(
    'Tour Đà Lạt 1',
    'Khám phá Hồ Xuân Hương và các địa điểm nổi tiếng tại Đà Lạt.',
    5000000,
    1,  -- ID của khách sạn Edensee Lake Resort & Spa
    1,  -- ID của nhà hàng Song Mây
    'dalat1.jpg',
    GETDATE()
),
(
    'Tour Đà Lạt 2',
    'Tham quan Thung Lũng Tình Yêu và thưởng thức ẩm thực địa phương.',
    6000000,
    2,  -- ID của khách sạn Ana Villas Đà Lạt Resort & Spa
    2,  -- ID của nhà hàng Ngọc Lan
    'dalat2.jpg',
    GETDATE()
);

INSERT INTO WishlistDeparture (
    wishlist_id, departure_date, price, available_slots
) VALUES 
(
    1,  -- ID của Tour Đà Lạt 1
    '2024-12-01',
    5000000,
    20
),
(
    2,  -- ID của Tour Đà Lạt 2
    '2024-12-10',
    6000000,
    15
);

INSERT INTO WishlistServices (wishlist_id, service_id) 
VALUES 
(1, 1),  -- Tour Đà Lạt 1 có dịch vụ Hướng dẫn viên du lịch
(1, 2),  -- Tour Đà Lạt 1 có dịch vụ Vé tham quan
(2, 1),  -- Tour Đà Lạt 2 có dịch vụ Hướng dẫn viên du lịch
(2, 3);  -- Tour Đà Lạt 2 có dịch vụ Ăn uống tại nhà hàng
GO

-- Thêm ngày khởi hành cho một combo tour
INSERT INTO TourComboDeparture (combo_id, departure_date, price,available_slots)
VALUES (1, '2025-2-20', 1590000,50),
       (1, '2025-11-15', 1690000,14),
       (1, '2024-12-30', 1690000,15),
       (2, '2024-12-25', 1790000,10);

go
CREATE TRIGGER UpdateAverageRating
ON Tour_Review
AFTER INSERT, DELETE, UPDATE
AS
BEGIN
    -- Cập nhật điểm đánh giá trung bình cho các tour có liên quan
    UPDATE t
    SET t.average_rating = 
        (
            SELECT 
                CASE 
                    WHEN COUNT(r.rating) = 0 THEN 0  -- Nếu không có đánh giá, gán giá trị 0
                    ELSE ROUND(AVG(CAST(r.rating AS FLOAT)) * 2, 0) / 2 -- Làm tròn đến nửa điểm gần nhất
                END
            FROM Tour_Review r
            WHERE r.tour_id = t.tour_id
        )
    FROM Tour t
    WHERE t.tour_id IN (SELECT DISTINCT tour_id FROM inserted)
       OR t.tour_id IN (SELECT DISTINCT tour_id FROM deleted);
END;
GO


DELETE FROM Booking;
DELETE FROM Customer;
DELETE FROM Hotel;
DELETE FROM ImgList;
DELETE FROM Invoice;
DELETE FROM Restaurant;
DELETE FROM Staff;
DELETE FROM Tour;
DELETE FROM Tour_Detail;
DELETE FROM Tour_Review;
DELETE FROM Tour_Services;
DELETE FROM TourCombo;
DELETE FROM TourComboDeparture;
DELETE FROM TourCombo_Services;
DELETE FROM WishlistServices;

DBCC CHECKIDENT ('Booking', RESEED, 0);
DBCC CHECKIDENT ('Customer', RESEED, 0);
DBCC CHECKIDENT ('Hotel', RESEED, 0);
DBCC CHECKIDENT ('ImgList', RESEED, 0);
DBCC CHECKIDENT ('Invoice', RESEED, 0);
DBCC CHECKIDENT ('Restaurant', RESEED, 0);
DBCC CHECKIDENT ('Staff', RESEED, 0);
DBCC CHECKIDENT ('Tour', RESEED, 0);
DBCC CHECKIDENT ('Tour_Review', RESEED, 0);
DBCC CHECKIDENT ('Tour_Services', RESEED, 0);
DBCC CHECKIDENT ('Tour_Detail', RESEED, 0);
DBCC CHECKIDENT ('Hotel', RESEED, 0);
DBCC CHECKIDENT ('Restaurant', RESEED, 0);
DBCC CHECKIDENT ('TourCombo', RESEED, 0);

DROP USER [trung@gmail.com];
DROP USER [nguyenvana@example.com];
DROP USER [tranthib@example.com];
GO
DROP LOGIN [trung@gmail.com];
DROP LOGIN [nguyenvana@example.com];
DROP LOGIN [tranthib@example.com];
GO
DROP ROLE AdminRole;
DROP ROLE StaffRole;
DROP ROLE Tour_Manager;
GO

select * from Booking
INSERT INTO Booking (customer_id, combo_id, departure_date, num_people, total_price, special_requests, booking_status, booking_date) 
VALUES (1, 2, '2024-10-20', 2, 3200000, 'No special requests', 'Pending', GETDATE());

use master
drop database DB_TourDuLich