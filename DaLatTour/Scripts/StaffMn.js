function togglePasswords() {
    const passwordFields = [document.getElementById("editStaffPassword"), document.getElementById("staffPassword")];
    const toggleButton = event.target;

    passwordFields.forEach(field => {
        if (field.type === "password") {
            field.type = "text";
            toggleButton.textContent = "Ẩn mật khẩu";
        } else {
            field.type = "password";
            toggleButton.textContent = "Hiển thị mật khẩu";
        }
    });
}

$(document).ready(function () {
    // Kích hoạt nút sửa và xóa khi chọn một nhân viên
    $('table').on('change', '.staffRadio', function () {
        $('#editStaffButton').prop('disabled', false);
        $('#deleteStaffButton').prop('disabled', false);
    });

    $('#editStaffModal .close, #editStaffModal .btn-secondary').on('click', function () {
        $('#editStaffModal').modal('hide');
    });

    $('#editStaffButton').click(function () {
        var selectedStaffId = $('input[name="selectedStaff"]:checked').val();
        if (selectedStaffId) {
            // Gọi API để kiểm tra phương thức Controller
            $.ajax({
                url: '/Admin/GetStaff/' + selectedStaffId,
                type: 'GET',
                success: function (response) {
                    if (response.success) {
                        // Kiểm tra dữ liệu trả về
                        console.log(response.data);
                        // Hiển thị dữ liệu lên modal
                        $('#editStaffName').val(response.data.name);
                        $('#editStaffEmail').val(response.data.email);
                        $('#editStaffPhone').val(response.data.phone);
                        $('#editStaffRole').val(response.data.username);
                        $('#editStaffPassword').val(response.data.password);
                        
                        $('#editStaffModal').modal('show');
                    } else {
                        alert(response.message);
                    }
                },
                error: function (xhr, status, error) {
                    console.log("Error: " + error);
                    console.log("Status: " + status);
                    console.dir(xhr);
                    alert('Không thể gọi phương thức Controller.');
                }
            });
        } else {
            alert('Vui lòng chọn nhân viên.');
        }
    });

    // Xử lý sự kiện nút xóa người dùng
    $('#deleteStaffButton').click(function () {
        var selectedStaffId = $('input[name="selectedStaff"]:checked').val();
        if (selectedStaffId && confirm('Bạn có chắc chắn muốn xóa nhân viên này?')) {
            // Gửi yêu cầu Ajax để xóa nhân viên
            $.ajax({
                url: '/Admin/DeleteStaff/' + selectedStaffId,
                type: 'POST',
                success: function (response) {
                    // Hiển thị thông báo thành công
                    alert(response.message || 'Xóa nhân viên thành công.');
                    // Thêm khoảng thời gian chờ trước khi tải lại trang
                    setTimeout(function () {
                        location.reload();
                    }, 250); // Chờ 1/4 giây trước khi tải lại trang
                },
                error: function () {
                    alert('Đã có lỗi xảy ra, vui lòng thử lại.');
                }
            });
        }
    });

    function validateStaffData(name, email, phone, role, password) {
        // Kiểm tra tên nhân viên
        if (!name || name.trim() === '') {
            alert('Tên nhân viên không được để trống.');
            return false;
        }

        // Kiểm tra email hợp lệ
        var emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/; // Biểu thức chính quy cho email
        if (!email || !emailPattern.test(email)) {
            alert('Email không hợp lệ. Vui lòng nhập địa chỉ email hợp lệ.');
            return false;
        }

        // Kiểm tra số điện thoại hợp lệ
        if (!phone || phone.trim() === '') {
            alert('Số điện thoại không được để trống.');
            return false;
        }

        var phonePattern = /^0\d{9}$/;
        if (!phonePattern.test(phone)) {
            alert('Số điện thoại không hợp lệ. Vui lòng nhập số điện thoại đúng định dạng.');
            return false;
        }

        // Kiểm tra vai trò
        if (!role) {
            alert('Vui lòng chọn vai trò.');
            return false;
        }

        // Kiểm tra mật khẩu
        if (!password || password.length < 6) {
            alert('Mật khẩu phải có ít nhất 6 ký tự.');
            return false;
        }

        // Nếu tất cả kiểm tra đều hợp lệ
        return true;
    }

    // Xử lý sự kiện khi nhấn nút Lưu Nhân Viên
    $('#saveStaff').click(function () {
        var staffName = $('#staffName').val();
        var staffEmail = $('#staffEmail').val();
        var staffPhone = $('#staffPhone').val();
        var staffRole = $('#staffRole').val();
        var staffPassword = $('#staffPassword').val();

        // Kiểm tra số điện thoại hợp lệ (ví dụ: 10 số và bắt đầu với 09)
        var phonePattern = /^0\d{9}$/;

        // Kiểm tra thông tin nhân viên
        if (!validateStaffData(staffName, staffEmail, staffPhone, staffRole, staffPassword)) {
            return;  // Dừng lại nếu có lỗi
        }

        // Kiểm tra số điện thoại hợp lệ
        if (staffPhone && !phonePattern.test(staffPhone)) {
            alert('Số điện thoại không hợp lệ. Vui lòng nhập số điện thoại đúng định dạng.');
            return;  // Dừng lại nếu số điện thoại không hợp lệ
        }

        // Tạo dữ liệu nhân viên
        var staffData = {
            name: staffName,
            email: staffEmail,
            phone: staffPhone,
            role: staffRole,
            password: staffPassword
        };

        // Gửi yêu cầu AJAX để tạo nhân viên mới
        $.ajax({
            url: '/Admin/CreateStaff',  // Thay đổi URL tới action Create của AdminController
            type: 'POST',
            data: JSON.stringify(staffData),  // Chuyển dữ liệu thành JSON
            contentType: 'application/json; charset=utf-8',  // Chỉ định kiểu nội dung là JSON
            dataType: 'json',  // Mong đợi phản hồi dạng JSON
            success: function (response) {
                if (response.success) {
                    alert('Thêm nhân viên thành công!');
                    $('#addStaffModal').modal('hide');
                    location.reload();
                } else {
                    // Kiểm tra thông báo lỗi từ server
                    if (response.message.includes("duplicate key")) {
                        alert('Email hoặc tài khoản này đã tồn tại. Vui lòng kiểm tra lại!');
                    } else {
                        alert('Đã có lỗi: ' + response.message);
                    }
                }
            },
            error: function () {
                alert('Đã có lỗi xảy ra, vui lòng thử lại.');
            }
        });
    });

    // Gọi hàm removeDiacritics (nếu chưa có hàm này ở trong code, bạn có thể đặt bên ngoài hàm searchStaff)
    function removeDiacritics(input) {
        return input.normalize("NFD").replace(/[\u0300-\u036f]/g, "");
    }

    // Gọi hàm tìm kiếm khi nhấn nút
    $('#searchStaffButton').click(searchStaff);

    // Gọi hàm tìm kiếm khi nhấn phím Enter
    $('#searchStaff').keypress(function (event) {
        if (event.which === 13) { // Mã phím Enter là 13
            event.preventDefault();
            searchStaff();
        }
    });

    function searchStaff() {
        var searchTerm = $('#searchStaff').val();
        console.log('Initial search term:', searchTerm);

        // Xóa dấu tiếng Việt khỏi searchTerm
        searchTerm = removeDiacritics(searchTerm);
        console.log('Search term after removing diacritics:', searchTerm);

        $.ajax({
            url: '/Admin/SearchStaffs',
            type: 'GET',
            data: { searchTerm: searchTerm },
            success: function (data) {
                var tableBody = $('table tbody');
                tableBody.empty();

                // Tắt các nút Xóa và Sửa sau khi load bảng mới
                $('#editStaffButton').prop('disabled', true);
                $('#deleteStaffButton').prop('disabled', true);

                $.each(data, function (index, staff) {
                    // Định dạng created_at từ dạng JSON /Date(xxxxxxxxxxxxx)/
                    var formattedCreatedAt = formatDate(staff.created_at);

                    console.log('Staff ID:', staff.staff_id);
                    console.log('Name:', staff.name);
                    console.log('Email:', staff.email);
                    console.log('Phone:', staff.phone);
                    console.log('Role:', staff.role);
                    console.log('Created At:', staff.created_at);
                    console.log('Formatted Created At:', formattedCreatedAt);

                    var row = '<tr>' +
                        '<td class="text-center"><input type="radio" name="selectedStaff" class="staffRadio" value="' + staff.staff_id + '"></td>' +
                        '<th scope="row" class="text-center">' + staff.staff_id + '</th>' +
                        '<td class="text-center">' + staff.name + '</td>' +
                        '<td class="text-center">' + staff.email + '</td>' +
                        '<td class="text-center">' + staff.phone + '</td>' +
                        '<td class="text-center">' + staff.role + '</td>' +
                        '<td class="text-center">******</td>' + // Hiển thị mật khẩu dưới dạng ẩn
                        '<td class="text-center">' + formattedCreatedAt + '</td>' + // Định dạng ngày tạo tài khoản
                        '</tr>';
                    tableBody.append(row);
                });
            },
            error: function (xhr, status, error) {
                console.error("Status: " + status);
                console.error("Error: " + error);
                console.error("Response: " + xhr.responseText);
                alert("Đã xảy ra lỗi: " + error);
            }
        });
    }

    // Hàm định dạng ngày từ chuỗi /Date(xxxxxxxxxxxxx)/
    function formatDate(dateString) {
        var timestamp = parseInt(dateString.match(/\d+/)[0]);
        var date = new Date(timestamp);
        return date.toLocaleString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit', second: '2-digit' });
    }


    // Xử lý sự kiện khi nhấn nút Cập Nhật
    $('#updateStaff').click(function () {
        var staffName = $('#editStaffName').val();
        var staffEmail = $('#editStaffEmail').val();
        var staffPhone = $('#editStaffPhone').val();
        var staffRole = $('#editStaffRole').val();
        var staffPassword = $('#editStaffPassword').val();
        var selectedStaffId = $('input[name="selectedStaff"]:checked').val(); // Lấy ID của nhân viên đang được chỉnh sửa

        // Kiểm tra thông tin nhân viên
        if (!validateStaffData(staffName, staffEmail, staffPhone, staffRole, staffPassword)) {
            return;  // Dừng lại nếu có lỗi
        }

        // Tạo dữ liệu nhân viên
        var userData = {
            id: selectedStaffId,  // ID của nhân viên
            name: staffName,
            email: staffEmail,
            phone: staffPhone,
            role: staffRole,
            password: staffPassword
        };

        // Gửi yêu cầu AJAX để cập nhật người dùng
        $.ajax({
            url: '/Admin/UpdateStaff',  // Thay đổi URL tới action Update của AdminController
            type: 'POST',
            data: JSON.stringify(userData),  // Chuyển dữ liệu thành JSON
            contentType: 'application/json; charset=utf-8',  // Chỉ định kiểu nội dung là JSON
            dataType: 'json',  // Mong đợi phản hồi dạng JSON
            success: function (response) {
                if (response.success) {
                    $('#editStaffModal').modal('hide');
                    alert(response.message);
                    location.reload(); // Tải lại trang để hiển thị thông tin mới
                } else {
                    // Kiểm tra thông báo lỗi từ server
                    if (response.message.includes("duplicate key")) {
                        alert('Email hoặc tài khoản này đã tồn tại. Vui lòng kiểm tra lại!');
                    } else {
                        alert('Đã có lỗi: ' + response.message);
                    }
                }
            },
            error: function () {
                alert('Đã có lỗi xảy ra, vui lòng thử lại.');
            }
        });
    });
});
