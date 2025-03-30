$(document).ready(function () {
    $('.slider').slick({
        autoplay: true,
        autoplaySpeed: 2000,
        dots: true,
        arrows: true,
        infinite: true,
        speed: 500,
        slidesToShow: 1,
        slidesToScroll: 1
    });
});

document.querySelectorAll(".nav-link").forEach(link => {
    // Lấy giá trị href và cắt khoảng trắng
    const href = (link.getAttribute("href") || "").trim();
    // Nếu href không bắt đầu bằng '#' thì bỏ qua (không gắn sự kiện)
    if (!href.startsWith("#")) {
        return;
    }

    link.addEventListener("click", function (event) {
        event.preventDefault();
        // Xóa lớp active của tất cả tab-pane và nav-link
        document.querySelectorAll(".tab-pane").forEach(tab => tab.classList.remove("show", "active"));
        document.querySelectorAll(".nav-link").forEach(nav => nav.classList.remove("active"));

        // Tìm phần tử có selector tương ứng
        const tabElement = document.querySelector(href);
        if (tabElement) {
            tabElement.classList.add("show", "active");
            this.classList.add("active");
        }
    });
});
