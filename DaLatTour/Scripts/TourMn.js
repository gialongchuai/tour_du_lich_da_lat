$(document).ready(function () {
    // Kích hoạt nút sửa và xóa khi chọn một tour
    $('table').on('change', '.tourRadio', function () {
        $('#editTourButton').prop('disabled', false);
        $('#deleteTourButton').prop('disabled', false);
    });

    $('#editTourModal .close, #editTourModal .btn-secondary').on('click', function() {
        $('#editTourModal').modal('hide');
    });

    $('#editTourButton').click(function () {
        var selectedTourId = $('input[name="selectedTour"]:checked').val();
        if (selectedTourId) {
            $.ajax({
                url: '/Tour/GetTour/' + selectedTourId,
                method: 'GET',
                success: function (data) {
                    $('#editTourName').val(data.tour_name);
                    $('#editDescription').val(data.description);
                    $('#editPrice').val(data.price);
                    $('#editDuration').val(data.duration);
                    $('#editTravelBy').val(data.travelby);
                    $('#editAvailableSlots').val(data.available_slots);

                    // Cập nhật tên tệp hiện tại
                    if (data.tour_image) {
                        $('#editTourImageLabel').text(data.tour_image);
                        $('#editTourImageCurrent').val(data.tour_image);
                    } else {
                        $('#editTourImageLabel').text('Chọn tệp');
                        $('#editTourImageCurrent').val('');
                    }

                    $('#editTourModal').modal('show');
                },
                error: function () {
                    alert('Có lỗi xảy ra khi lấy thông tin tour.');
                }
            });
        } else {
            alert('Vui lòng chọn một tour để sửa.');
        }
    });

    function validateTourData(tourName, description, price, duration, travelBy, availableSlots) {
        // Kiểm tra các trường bắt buộc có được điền không
        if (!tourName || !description || !price || !duration || !travelBy || !availableSlots) {
            alert('Vui lòng điền đầy đủ các trường bắt buộc!');
            return false; // Dừng lại nếu có trường trống
        }
        // Kiểm tra giá phải là số dương
        var priceValue = price;
        if (isNaN(priceValue) || priceValue <= 0) {
            alert('Giá phải là một số dương!');
            return false;
        }

        // Kiểm tra thời lượng phải là số dương
        var durationValue = duration;
        if (isNaN(durationValue) || durationValue <= 0) {
            alert('Thời lượng (ngày) phải là một số dương!');
            return false;
        }

        // Kiểm tra số chỗ còn trống 
        var availableSlotsValue = availableSlots;
        if (isNaN(availableSlotsValue) || availableSlotsValue < 0) {
            alert('Số lượng chỗ còn trống không hợp lệ!');
            return false;
        }
        
        return true;
    }

    $('#saveEditedTour').click(function () {
        // Lấy giá trị từ các trường nhập liệu
        var tourName = $('#editTourName').val().trim();
        var description = $('#editDescription').val().trim();
        var price = $('#editPrice').val().trim();
        var duration = $('#editDuration').val().trim();
        var travelBy = $('#editTravelBy').val().trim();
        var availableSlots = $('#editAvailableSlots').val().trim();
        var currentImage = $('#editTourImageCurrent').val();
        var selectedTourId = $('input[name="selectedTour"]:checked').val(); // Lấy id của tour đang sửa

        if (!validateTourData(tourName, description, price, duration, travelBy, availableSlots)) {
            return;  // Dừng lại nếu có lỗi
        }

        var fileInput = $('#editTourImage'); // Truy cập phần tử DOM thực tế
        var file = fileInput[0].files[0]; // Lấy tệp ảnh mới nếu có

        var formData = new FormData();
        formData.append('tour_id', selectedTourId); // Gửi tour_id
        formData.append('tour_name', tourName);
        formData.append('description', description);
        formData.append('price', price);
        formData.append('duration', duration);
        formData.append('travelby', travelBy);
        formData.append('available_slots', availableSlots);
        formData.append('tour_image_current', currentImage); // Ảnh hiện tại

        // Nếu người dùng chọn ảnh mới, thêm ảnh vào formData
        if (file) {
            // Kiểm tra định dạng ảnh
            var allowedExtensions = /(\.jpg|\.jpeg|\.png)$/i;
            if (!allowedExtensions.exec(file.name)) {
                alert('Chỉ chấp nhận các tệp định dạng JPG hoặc PNG.');
                fileInput.val(''); // Reset trường ảnh
                return;
            }
            formData.append('tour_image', file); // Thêm tệp ảnh mới vào formData
        }

        // Gửi yêu cầu AJAX để cập nhật tour
        $.ajax({
            url: '/Tour/Update', // Đường dẫn đến action cập nhật trong controller
            type: 'POST',
            processData: false,
            contentType: false,
            data: formData,
            success: function (response) {
                if (response.success) {
                    alert(response.message);
                    $('#editTourModal').modal('hide');
                    location.reload(); // Tải lại trang sau khi cập nhật tour thành công
                } else {
                    alert(response.message);
                }
            },
            error: function () {
                alert('Đã có lỗi xảy ra, vui lòng thử lại.');
            }
        });
    });


    // Cập nhật label khi chọn file mới
    $('#editTourImage').on('change', function () {
        var fileName = $(this).val().split('\\').pop();
        $(this).next('.custom-file-label').text(fileName || 'Chọn tệp');
    });

    // Xử lý sự kiện nút xóa tour
    $('#deleteTourButton').click(function () {
        var selectedTourId = $('input[name="selectedTour"]:checked').val();
        if (selectedTourId && confirm('Bạn có chắc chắn muốn xóa tour này?')) {
            // Gửi yêu cầu Ajax để xóa tour
            $.ajax({
                url: '/Tour/DeleteTour/' + selectedTourId, // Gọi đúng action trong controller
                type: 'POST',
                success: function (response) {
                    if (response.success) {
                        // Hiển thị thông báo thành công
                        alert(response.message || 'Xóa tour thành công.');
                        // Thêm khoảng thời gian chờ trước khi tải lại trang
                        setTimeout(function () {
                            location.reload();
                        }, 250); // Chờ 1/4 giây trước khi tải lại trang
                    } else {
                        alert(response.message || 'Đã có lỗi xảy ra, vui lòng thử lại.');
                    }
                },
                error: function () {
                    alert('Đã có lỗi xảy ra, vui lòng thử lại.');
                }
            });
        }
    });

    $('#saveTour').click(function () {
        // Lấy giá trị từ các trường nhập liệu
        var tourName = $('#tourName').val().trim();
        var description = $('#description').val().trim();
        var price = $('#price').val().trim();
        var duration = $('#duration').val().trim();
        var travelBy = $('#travelBy').val().trim();
        var availableSlots = $('#availableSlots').val().trim();

        if (!validateTourData(tourName, description, price, duration, travelBy, availableSlots)) {
            return;  // Dừng lại nếu có lỗi
        }

        var fileInput = $('#tourImage'); // Truy cập phần tử DOM thực tế
        var file = fileInput[0].files[0]; // Lấy tệp ảnh đầu tiên

        // Kiểm tra xem người dùng đã chọn ảnh chưa
        if (!file) {
            alert('Vui lòng chọn một ảnh!');
            return;
        }

        // Kiểm tra định dạng ảnh
        var allowedExtensions = /(\.jpg|\.jpeg|\.png)$/i; // Chỉ cho phép jpg, jpeg, png
        if (!allowedExtensions.exec(file.name)) {
            alert('Chỉ chấp nhận các tệp định dạng JPG hoặc PNG.');
            fileInput.val(''); // Reset trường ảnh
            return;
        }

        var formData = new FormData();
        formData.append('tour_name', tourName);
        formData.append('description', description);
        formData.append('price', price);
        formData.append('duration', duration);
        formData.append('travelby', travelBy);
        formData.append('available_slots', availableSlots);
        formData.append('tour_image', file); // Thêm tệp ảnh vào FormData

        $.ajax({
            url: '/Tour/Create', // Đường dẫn đến action trong controller
            type: 'POST',
            processData: false,
            contentType: false,
            data: formData,
            success: function (response) {
                if (response.success) {
                    alert(response.message);
                    $('#addTourModal').modal('hide');
                    location.reload(); // Tải lại trang sau khi thêm tour thành công
                } else {
                    alert(response.message);
                }
            },
            error: function () {
                alert('Đã có lỗi xảy ra, vui lòng thử lại.');
            }
        });
    });

    function removeDiacritics(input) {
        return input.normalize("NFD").replace(/[\u0300-\u036f]/g, "");
    };

    // Gọi hàm tìm kiếm khi nhấn nút
    $('#searchTourButton').click(searchTour);

    // Gọi hàm tìm kiếm khi nhấn phím Enter
    $('#searchTour').keypress(function (event) {
        if (event.which === 13) { // Mã phím Enter là 13
            event.preventDefault();
            searchTour();
        }
    });

    function searchTour() {
        var searchTerm = $('#searchTour').val();
        searchTerm = removeDiacritics(searchTerm);
        $.ajax({
            url: '/Tour/SearchTours',
            type: 'GET',
            data: { searchTerm: searchTerm },
            success: function (data) {
                var tableBody = $('table tbody');
                tableBody.empty();

                // Tắt các nút Xóa và Sửa sau khi load bảng mới
                $('#editTourButton').prop('disabled', true);
                $('#deleteTourButton').prop('disabled', true)
                
                $.each(data, function (index, tour) {
                    var row = '<tr>' +
                        '<td class="text-center"><input type="radio" name="selectedTour" class="tourRadio" value="' + tour.tour_id + '"></td>' +
                        '<th scope="row" class="text-center">' + tour.tour_id + '</th>' +
                        '<td class="text-center">' + tour.tour_name + '</td>' +
                        '<td class="text-center">' + tour.description + '</td>' +
                        '<td class="text-center">' + parseFloat(tour.price).toLocaleString('vi-VN', { style: 'currency', currency: 'VND' }) + '</td>' +
                        '<td class="text-center">' + tour.duration + '</td>' +
                        '<td class="text-center">' + tour.travelby + '</td>' +
                        '<td class="text-center">' + tour.available_slots + '</td>' +
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
    };
});
