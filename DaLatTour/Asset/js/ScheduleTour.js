$(document).ready(function () {
    // Kích hoạt nút sửa và xóa khi chọn một lịch trình
    $('.scheduleRadio').change(function () {
        $('#editScheduleButton').prop('disabled', false);
        $('#deleteScheduleButton').prop('disabled', false);
    });

    // Thêm lịch trình
    $('#saveSchedule').click(function () {
        const tourType = $('#tourType').val(); // Lấy loại tour từ dropdown
        const selectedTourId = $('#tourSelect').val(); // ID của tour đã chọn
        const departureDate = $('#departureDate').val();
        const numPeople = $('#numPeople').val();
        const price = $('#price').val();
        const returnDate = $('#returnDate').val();
        const dailyActivities = $('#dailyActivities').val();

        // Kiểm tra tính hợp lệ của dữ liệu
        if (!selectedTourId || !departureDate || !numPeople || (tourType === 'Combo' && (!price || price <= 0))) {
            alert('Vui lòng điền đầy đủ thông tin.');
            return;
        }

        const scheduleData = {
            tourId: tourType === 'Regular' ? selectedTourId : null,
            comboId: tourType === 'Combo' ? selectedTourId : null,
            departureDate: departureDate,
            returnDate: tourType === 'Regular' ? returnDate : null,
            availableSlots: numPeople,
            price: tourType === 'Combo' ? price : null,
            dailyActivities: tourType === 'Regular' ? dailyActivities : null
        };

        $.ajax({
            url: '/Admin/AddSchedule',
            type: 'POST',
            data: JSON.stringify(scheduleData),
            contentType: 'application/json',
            success: function (response) {
                if (response.success) {
                    alert('Thêm lịch trình thành công!');
                    $('#addScheduleModal').modal('hide');
                    location.reload(); // Tải lại trang sau khi thêm thành công
                } else {
                    alert('Có lỗi xảy ra: ' + response.message);
                }
            },
            error: function () {
                alert('Đã xảy ra lỗi khi thêm lịch trình.');
            }
        });
    });

    // Sửa lịch trình
    $('#editScheduleButton').click(function () {
        const selectedScheduleId = $('input[name="selectedSchedule"]:checked').val();
        const tourType = $('input[name="selectedSchedule"]:checked').data('tour-type');

        if (selectedScheduleId && tourType) {
            $.ajax({
                url: '/Admin/GetScheduleDetail',
                type: 'GET',
                data: { id: selectedScheduleId, tourType: tourType },
                success: function (response) {
                    if (response.success) {
                        $('#editDepartureDate').val(response.data.DepartureDate.split('T')[0]);

                        if (tourType === 'Regular') {
                            $('#editReturnDate').val(response.data.ReturnDate.split('T')[0]);
                            $('#editNumPeople').val(response.data.NumPeople);
                            $('#editDailyActivities').val(response.data.DailyActivities);
                            $('#returnDateField, #dailyActivitiesField').show();
                            $('#priceField').hide();
                        } else if (tourType === 'Combo') {
                            $('#editNumPeople').val(response.data.AvailableSlots);
                            $('#editPrice').val(response.data.Price);
                            $('#returnDateField, #dailyActivitiesField').hide();
                            $('#priceField').show();
                        }

                        $('#editScheduleModal').modal('show');
                    } else {
                        alert(response.message);
                    }
                },
                error: function () {
                    alert('Không thể tải thông tin lịch trình.');
                }
            });
        } else {
            alert('Vui lòng chọn một lịch trình để sửa.');
        }
    });

    // Cập nhật lịch trình
    $('#updateSchedule').click(function () {
        const selectedScheduleId = $('input[name="selectedSchedule"]:checked').val();
        const tourType = $('input[name="selectedSchedule"]:checked').data('tour-type');
        const departureDate = $('#editDepartureDate').val();
        const availableSlots = $('#editNumPeople').val();
        const price = $('#editPrice').val();
        const returnDate = $('#editReturnDate').val();
        const dailyActivities = $('#editDailyActivities').val();

        const scheduleData = {
            TourId: tourType === 'Regular' ? selectedScheduleId : null,
            ComboId: tourType === 'Combo' ? selectedScheduleId : null,
            DepartureDate: departureDate,
            ReturnDate: tourType === 'Regular' ? returnDate : null,
            AvailableSlots: availableSlots,
            Price: tourType === 'Combo' ? price : null,
            DailyActivities: tourType === 'Regular' ? dailyActivities : null,
            TourType: tourType
        };

        $.ajax({
            url: '/Admin/UpdateSchedule',
            type: 'POST',
            data: JSON.stringify(scheduleData),
            contentType: 'application/json',
            success: function (response) {
                if (response.success) {
                    alert('Cập nhật lịch trình thành công!');
                    $('#editScheduleModal').modal('hide');
                    location.reload();
                } else {
                    alert('Có lỗi xảy ra: ' + response.message);
                }
            },
            error: function () {
                alert('Đã xảy ra lỗi khi cập nhật lịch trình.');
            }
        });
    });

    // Xóa lịch trình
    $('#deleteScheduleButton').click(function () {
        var selectedScheduleId = $('input[name="selectedSchedule"]:checked').val();  // Lấy ID của lịch trình đã chọn
        var tourType = $('input[name="selectedSchedule"]:checked').data('tour-type'); // Lấy loại tour (Regular hoặc Combo)

        if (selectedScheduleId && tourType && confirm('Bạn có chắc chắn muốn xóa lịch trình này?')) {
            var url = tourType === 'Regular' ? '/Admin/DeleteRegularTour/' : '/Admin/DeleteComboTour/';

            $.ajax({
                url: url + selectedScheduleId,
                type: 'POST',
                success: function (response) {
                    if (response.success) {
                        alert('Xóa lịch trình thành công!');
                        location.reload(); // Tải lại trang sau khi xóa thành công
                    } else {
                        alert('Có lỗi xảy ra: ' + response.message);
                    }
                },
                error: function () {
                    alert('Đã xảy ra lỗi khi xóa lịch trình.');
                }
            });
        }
    });
});

// Tải danh sách tour dựa trên loại tour đã chọn
function loadTourList(tourType) {
    $.ajax({
        url: '/Admin/GetToursByType',
        type: 'GET',
        data: { type: tourType },
        success: function (response) {
            const tourSelect = $('#tourSelect');
            tourSelect.empty();
            if (response.success) {
                response.data.forEach(function (tour) {
                    tourSelect.append(new Option(tour.name, tour.id));
                });
            } else {
                tourSelect.append(new Option('Không có tour nào', ''));
            }
        },
        error: function () {
            alert('Đã xảy ra lỗi khi tải danh sách tour.');
        }
    });

    toggleFields({ value: tourType });
}


// Hiển thị bảng tương ứng dựa trên lựa chọn từ dropdown
function toggleTourTables() {
    const selectedType = document.getElementById("tourTypeDropdown").value;
    const regularTable = document.getElementById("regularToursTable");
    const comboTable = document.getElementById("comboToursTable");

    if (selectedType === "regular") {
        regularTable.style.display = "block";
        comboTable.style.display = "none";
    } else if (selectedType === "combo") {
        regularTable.style.display = "none";
        comboTable.style.display = "block";
    }
}

// Khởi tạo mặc định hiển thị tất cả các bảng
document.addEventListener("DOMContentLoaded", function () {
    toggleTourTables();
});
function toggleFields(selectedTourType) {
    const returnDateField = document.getElementById('returnDateField');
    const numPeopleField = document.getElementById('numPeopleField');
    const priceField = document.getElementById('priceField');
    const dailyActivitiesField = document.getElementById('dailyActivitiesField');

    if (selectedTourType.value === 'Regular') {
        returnDateField.style.display = 'block';
        numPeopleField.style.display = 'block';
        dailyActivitiesField.style.display = 'block';
        priceField.style.display = 'none';
    } else if (selectedTourType.value === 'Combo') {
        returnDateField.style.display = 'none';
        numPeopleField.style.display = 'block';
        dailyActivitiesField.style.display = 'none';
        priceField.style.display = 'block';
    }
}
