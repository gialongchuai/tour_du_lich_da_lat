function togglePasswords() {
    const passwordFields = [document.getElementById("editUserPassword"), document.getElementById("userPassword")];
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
    // Kích hoạt nút sửa và xóa khi chọn một người dùng

    $('table').on('change', '.userRadio', function () {
        $('#editUserButton').prop('disabled', false);
        $('#deleteUserButton').prop('disabled', false);
    });

    $('#editUserModal .close, #editUserModal .btn-secondary').on('click', function () {
        $('#editUserModal').modal('hide');
    });

    $('#editUserButton').click(function () {
        var selectedUserId = $('input[name="selectedUser"]:checked').val();
        if (selectedUserId) {
            // Gọi API để kiểm tra phương thức Controller
            $.ajax({
                url: '/Admin/GetUser/' + selectedUserId,
                type: 'GET',
                success: function (response) {
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
                        if (response.data.dob) {
                            // Trích xuất timestamp từ chuỗi "/Date(1729875600000)/"
                            var dobTimestamp = parseInt(response.data.dob.replace(/\/Date\((\d+)\)\//, '$1'), 10);
                            var dob = new Date(dobTimestamp);

                            // Kiểm tra xem đối tượng Date có hợp lệ không
                            if (!isNaN(dob.getTime())) {
                                dob.setDate(dob.getDate() + 1); // Cộng thêm 1 ngày
                                $('#editUserDob').val(dob.toISOString().split('T')[0]); // Chỉ lấy phần ngày theo định dạng yyyy-MM-dd
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
                error: function (xhr, status, error) {
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

        // Kiểm tra mật khẩu
        if (!password || password.length < 6) {
            alert('Mật khẩu phải có ít nhất 6 ký tự.');
            return false;
        }

        return true; // Tất cả đều hợp lệ
    }

    // Xử lý sự kiện khi nhấn nút Cập Nhật
    $('#updateUser').click(function () {
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
            success: function (response) {
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
            error: function () {
                alert('Đã có lỗi xảy ra, vui lòng thử lại.');
            }
        });
    });
    ;

    // Xử lý sự kiện nút xóa người dùng
    $('#deleteUserButton').click(function () {
        var selectedUserId = $('input[name="selectedUser"]:checked').val();
        if (selectedUserId && confirm('Bạn có chắc chắn muốn xóa người dùng này?')) {
            // Gửi yêu cầu Ajax để xóa người dùng
            $.ajax({
                url: '/Admin/DeleteUser/' + selectedUserId,
                type: 'POST',
                success: function (response) {
                    // Hiển thị thông báo thành công
                    alert(response.message || 'Xóa người dùng thành công.');
                    // Thêm khoảng thời gian chờ trước khi tải lại trang
                    setTimeout(function () {
                        location.reload();
                    }, 250); // Chờ 1/4 giây trước khi tải lại trang
                },
                error: function () {
                    alert('Đã có lỗi xảy ra, vui lòng thử lại.'); 1;
                }
            });
        }
    });

    function removeDiacritics(input) {
        return input.normalize("NFD").replace(/[\u0300-\u036f]/g, "");
    }

    // Gọi hàm tìm kiếm khi nhấn nút
    $('#searchUserButton').click(searchUser);

    // Gọi hàm tìm kiếm khi nhấn phím Enter
    $('#searchUser').keypress(function (event) {
        if (event.which === 13) { // Mã phím Enter là 13
            event.preventDefault();
            searchUser();
        }
    });

    function searchUser() {
        var searchTerm = $('#searchUser').val();
        searchTerm = removeDiacritics(searchTerm);

        $.ajax({
            url: '/Admin/SearchCustomers',
            type: 'GET',
            data: { searchTerm: searchTerm },
            success: function (data) {
                var tableBody = $('table tbody');
                tableBody.empty();

                // Tắt các nút Xóa và Sửa sau khi load bảng mới
                $('#editUserButton').prop('disabled', true);
                $('#deleteUserButton').prop('disabled', true);

                $.each(data, function (index, user) {

                    // Chuyển đổi định dạng ngày tháng
                    var formattedDob = formatDate(user.dob, true); // true để chỉ định dạng ngày
                    var formattedCreatedAt = formatDate(user.created_at, false); // false để hiển thị cả ngày và giờ

                    var row = '<tr>' +
                        '<td class="text-center"><input type="radio" name="selectedUser" class="userRadio" value="' + user.customer_id + '"></td>' +
                        '<th scope="row" class="text-center">' + user.customer_id + '</th>' +
                        '<td class="text-center">' + user.name + '</td>' +
                        '<td class="text-center">' + user.email + '</td>' +
                        '<td class="text-center">' + user.phone + '</td>' +
                        '<td class="text-center">' + user.username + '</td>' +
                        '<td class="text-center">******</td>' + // Hiển thị mật khẩu dưới dạng ẩn
                        '<td class="text-center">' + user.address + '</td>' +
                        '<td class="text-center">' + formattedDob + '</td>' +
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

    // Hàm chuyển đổi định dạng ngày tháng
    function formatDate(timestamp, isDob) {
        if (timestamp) {
            // Chuyển đổi timestamp về ngày tháng
            var date = new Date(parseInt(timestamp.substr(6)));
            var day = String(date.getDate()).padStart(2, '0');
            var month = String(date.getMonth() + 1).padStart(2, '0'); // Tháng bắt đầu từ 0
            var year = date.getFullYear();

            if (isDob) {
                // Chỉ định dạng ngày sinh (không có giờ)
                return `${day}/${month}/${year}`;
            } else {
                // Định dạng ngày tạo tài khoản (có giờ)
                var hours = String(date.getHours()).padStart(2, '0');
                var minutes = String(date.getMinutes()).padStart(2, '0');
                return `${day}/${month}/${year} ${hours}:${minutes}`;
            }
        }
        return '';
    }


    // Xử lý sự kiện khi nhấn nút Lưu
    $('#saveUser').click(function () {
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
            success: function (response) {
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
            error: function () {
                alert('Đã có lỗi xảy ra, vui lòng thử lại.');
            }
        });
    });
});