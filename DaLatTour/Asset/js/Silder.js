document.addEventListener("DOMContentLoaded", function () {
    const slides = document.querySelectorAll(".slider .slide");
    let currentIndex = 0;

    // Hàm để chuyển đổi slide
    function changeSlide() {
        // Ẩn slide và văn bản hiện tại
        slides[currentIndex].classList.remove("active");
        const currentText = slides[currentIndex].querySelector(".video-text");
        currentText.classList.remove("active"); // Ẩn chữ hiện tại

        // Tính chỉ số slide tiếp theo
        currentIndex = (currentIndex + 1) % slides.length;

        // Hiển thị slide và văn bản tiếp theo
        slides[currentIndex].classList.add("active");
        const nextText = slides[currentIndex].querySelector(".video-text");
        setTimeout(() => {
            nextText.classList.add("active"); // Hiển thị chữ của slide mới
        }, 200); // Độ trễ để tạo cảm giác chuyển động mượt hơn

        // Tự động phát video tiếp theo
        const nextVideo = slides[currentIndex].querySelector("video");
        nextVideo.play().catch(error => {
            console.error("Video không thể phát: ", error);
        });
    }

    // Khởi động slide đầu tiên
    slides[currentIndex].classList.add("active");
    const firstText = slides[currentIndex].querySelector(".video-text");
    firstText.classList.add("active"); // Hiển thị chữ đầu tiên
    const firstVideo = slides[currentIndex].querySelector("video");
    firstVideo.play().catch(error => {
        console.error("Video không thể phát: ", error);
    });

    // Bắt đầu quá trình chuyển đổi slide
    setInterval(changeSlide, 10000); // Thay đổi sau mỗi 10 giây
});
