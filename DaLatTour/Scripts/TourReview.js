$(document).ready(function () {
    var tourData = []; // Mảng lưu thông tin tour, bao gồm cả hình ảnh

    $('#addReviewButton').click(function () {
        $.ajax({
            url: '/Tour/CheckLoginStatus',
            method: 'GET',
            success: function (response) {
                if (response.isLoggedIn) {
                    $('#addReviewModal').modal('show'); // Mở modal nếu đã đăng nhập

                    // Gọi AJAX để lấy danh sách tour và điền vào dropdown
                    $.ajax({
                        url: '/Tour/GetTours',
                        method: 'GET',
                        success: function (data) {
                            tourData = data; // Lưu dữ liệu tour vào mảng để sử dụng sau

                            var tourSelect = $('#tourSelect');
                            tourSelect.empty(); // Xóa các option cũ
                            tourSelect.append('<option value="">Chọn tour</option>'); // Option mặc định

                            // Thêm các option mới từ dữ liệu
                            $.each(data, function (index, tour) {
                                tourSelect.append('<option value="' + tour.TourId + '">' + tour.TourName + '</option>');
                            });
                        },
                        error: function () {
                            alert("Đã xảy ra lỗi khi tải danh sách tour. Vui lòng thử lại.");
                        }
                    });
                } else {
                    alert("Vui lòng đăng nhập trước khi đánh giá tour.");
                }
            },
            error: function () {
                alert("Đã xảy ra lỗi khi kiểm tra trạng thái đăng nhập. Vui lòng thử lại.");
            }
        });
    });

    // Xử lý sự kiện khi chọn tour trong dropdown
    $('#tourSelect').change(function () {
        var selectedTourId = $(this).val(); // Lấy tour ID được chọn
        var tourImageElement = $('#tourImage'); // Phần tử ảnh

        if (selectedTourId) {
            // Tìm tour tương ứng trong dữ liệu
            var selectedTour = tourData.find(tour => tour.TourId == selectedTourId);

            if (selectedTour) {
                // Đặt thuộc tính src của ảnh và hiển thị ảnh
                tourImageElement.attr('src', '/Asset/img/' + selectedTour.TourImage);
                tourImageElement.show();
            }
        } else {
            // Ẩn ảnh nếu không có tour nào được chọn
            tourImageElement.hide();
            tourImageElement.attr('src', ''); // Xóa thuộc tính src
        }
    });

    $('#saveReview').click(function () {
        // Lấy các giá trị từ form
        var selectedTourId = $('#tourSelect').val();
        var rating = $('#ratingSelect').val();
        var reviewText = $('#reviewText').val();

        // Kiểm tra validate
        if (!selectedTourId) {
            alert('Vui lòng chọn Tour!');
            $('#tourSelect').focus();
            return;
        }

        if (!rating) {
            alert('Vui lòng chọn số sao đánh giá!');
            $('#ratingSelect').focus();
            return;
        }

        // Tạo object chứa dữ liệu đánh giá
        var reviewData = {
            TourId: selectedTourId,
            Rating: rating,
            ReviewText: reviewText || '' // Nếu không có text thì gửi chuỗi rỗng
        };

        // Gọi AJAX để lưu đánh giá
        $.ajax({
            url: '/Tour/SaveReview',
            method: 'POST',
            data: reviewData,
            success: function (response) {
                if (response.success) {
                    // Đóng modal
                    $('#addReviewModal').modal('hide');

                    // Hiển thị thông báo thành công
                    alert(response.message || 'Đánh giá tour đã được lưu thành công!');

                    // Reset form
                    $('#addReviewForm')[0].reset();
                    $('#tourImage').hide();

                    // Reload lại trang
                    window.location.href = '/Tour/TourReview';
                } else {
                    alert(response.message || 'Không thể lưu đánh giá tour. Vui lòng thử lại!');
                }
            },
            error: function (xhr, status, error) {
                alert('Đã xảy ra lỗi khi gửi đánh giá. Vui lòng thử lại!');
            }
        });
    });
});
