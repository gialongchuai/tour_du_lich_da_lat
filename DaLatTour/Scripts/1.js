$(document).ready(function() {
    // Kích hoạt nút sửa và xóa khi chọn một người dùng
    $('.userRadio').change(function() {
        $('#editUserButton').prop('disabled', false);
        $('#deleteUserButton').prop('disabled', false);
    });

    $('#editUserModal .close, #editUserModal .btn-secondary').on('click', function() {
        $('#editUserModal').modal('hide');
    });

    $('#editUserButton').click(function() {
        var selectedUserId = $('input[name="selectedUser"]:checked').val();
        if (selectedUserId) {
            // Gọi API để kiểm tra phương thức Controller
            $.ajax({
                url: '/Admin/GetUser/' + selectedUserId,
                type: 'GET',
                success: function(response) {
                    if (response.success) {
                        // Kiểm tra dữ liệu trả về
                        console.log(response.data);
                        // Hiển thị dữ liệu lên modal
                        $('#editName').val(response.data.name);
                        $('#editUserEmail').val(response.data.email);
                        $('#editUserPhone').val(response.data.phone);
                        $('#editUserName').val(response.data.username);
                        $('#editUserPassword').val(response.data.password);
                        $('#editUserAddress').val(response.data.address);
                        // Kiểm tra và hiển thị ngày sinh
                        if (response.data.dob) {
                            // Trích xuất timestamp từ chuỗi "/Date(698086800000)/"
                            var dobTimestamp = parseInt(response.data.dob.replace(/\/Date\((\d+)\)\//, '$1'), 10);
                            var dob = new Date(dobTimestamp);

                            if (!isNaN(dob.getTime())) { // Kiểm tra xem đối tượng Date có hợp lệ không
                                $('#editUserDob').val(dob.toISOString().split('T')[0]);
                            } else {
                                console.warn("Ngày sinh không hợp lệ:", response.data.dob);
                                $('#editUserDob').val(''); // Nếu không hợp lệ, đặt trống
                            }
                        } else {
                            $('#editUserDob').val(''); // Nếu không có, đặt trống
                        }
                        $('#editUserModal').modal('show');
                    } else {
                        alert(response.message);
                    }
                },
                error: function(xhr, status, error) {
                    console.log("Error: " + error);
                    console.log("Status: " + status);
                    console.dir(xhr);
                    alert('Không thể gọi phương thức Controller.');
                }
            });
        } else {
            alert('Vui lòng chọn người dùng.');
        }
    });


    // Hàm kiểm tra thông tin người dùng
    function validateUserData(name, email, phone, username, password, address, dob) {
        var phonePattern = /^0\d{9}$/;

        if (!name || !email || !phone || !username || !password || !address || !dob) {
            alert('Vui lòng điền đầy đủ thông tin.');
            return false; // Không hợp lệ
        }

        if (!phonePattern.test(phone)) {
            alert('Số điện thoại không hợp lệ. Vui lòng nhập số điện thoại đúng định dạng.');
            return false; // Không hợp lệ
        }

        return true; // Tất cả đều hợp lệ
    }

    // Xử lý sự kiện khi nhấn nút Cập Nhật
    $('#updateUser').click(function() {
        var name = $('#editName').val();
        var email = $('#editUserEmail').val();
        var phone = $('#editUserPhone').val();
        var username = $('#editUserName').val();
        var password = $('#editUserPassword').val();
        var address = $('#editUserAddress').val();
        var dob = $('#editUserDob').val();
        var selectedUserId = $('input[name="selectedUser"]:checked').val(); // Lấy ID của người dùng đang được chỉnh sửa




        // Kiểm tra thông tin người dùng
        if (!validateUserData(name, email, phone, username, password, address, dob)) {
            return; // Dừng lại nếu có lỗi
        }

        // Tạo dữ liệu người dùng
        var userData = {
            id: selectedUserId,
            name: name,
            email: email,
            phone: phone,
            username: username,
            password: password,
            address: address,
            dob: dob
        };

        // Gửi yêu cầu AJAX để cập nhật người dùng
        $.ajax({
            url: '/Admin/UpdateUser',
            type: 'POST',
            data: JSON.stringify(userData),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            success: function(response) {
                if (response.success) {
                    $('#editUserModal').modal('hide');
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
            error: function() {
                alert('Đã có lỗi xảy ra, vui lòng thử lại.');
            }
        });
    });
    ``;
    // Xử lý sự kiện nút xóa người dùng
    $('#deleteUserButton').click(function() {
        var selectedUserId = $('input[name="selectedUser"]:checked').val();
        if (selectedUserId && confirm('Bạn có chắc chắn muốn xóa người dùng này?')) {
            // Gửi yêu cầu Ajax để xóa người dùng
            $.ajax({
                url: '/Admin/DeleteUser/' + selectedUserId,
                type: 'POST',
                success: function(response) {
                    // Hiển thị thông báo thành công
                    alert(response.message || 'Xóa người dùng thành công.');
                    // Thêm khoảng thời gian chờ trước khi tải lại trang
                    setTimeout(function() {
                        location.reload();
                    }, 250); // Chờ 1/4 giây trước khi tải lại trang
                },
                error: function() {
                    alert('Đã có lỗi xảy ra, vui lòng thử lại.'); 1;
                }
            });
        }
    });

    // Xử lý sự kiện khi nhấn nút Lưu
    $('#saveUser').click(function() {
        var name = $('#name').val();
        var email = $('#userEmail').val();
        var phone = $('#userPhone').val();
        var username = $('#userName').val();
        var password = $('#userPassword').val();
        var address = $('#userAddress').val();
        var dob = $('#userDob').val();

        // Kiểm tra số điện thoại hợp lệ (ví dụ: 10 số và bắt đầu với 09)
        var phonePattern = /^0\d{9}$/;

        // Kiểm tra thông tin người dùng
        if (!validateUserData(name, email, phone, username, password, address, dob)) {
            return; // Dừng lại nếu có lỗi
        }

        // Kiểm tra số điện thoại hợp lệ
        if (!phonePattern.test(phone)) {
            alert('Số điện thoại không hợp lệ. Vui lòng nhập số điện thoại đúng định dạng.');
            return; // Dừng lại nếu số điện thoại không hợp lệ
        }

        // Kiểm tra ngày sinh có được chọn chưa
        if (!dob) {
            alert('Vui lòng chọn ngày sinh.');
            return; // Dừng lại nếu chưa chọn ngày sinh
        }

        // Tạo dữ liệu người dùng
        var userData = {
            name: name,
            email: email,
            phone: phone,
            username: username,
            password: password,
            address: address,
            dob: dob
        };

        // Gửi yêu cầu AJAX để tạo người dùng mới
        $.ajax({
            url: '/Admin/CreateUser',
            type: 'POST',
            data: JSON.stringify(userData),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            success: function(response) {
                if (response.success) {
                    alert('Thêm người dùng thành công!');
                    $('#addUserModal').modal('hide');
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
            error: function() {
                alert('Đã có lỗi xảy ra, vui lòng thử lại.');
            }
        });
    });
});
