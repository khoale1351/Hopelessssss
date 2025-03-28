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
    link.addEventListener("click", function (event) {
        event.preventDefault();
        document.querySelectorAll(".tab-pane").forEach(tab => tab.classList.remove("show", "active"));
        document.querySelectorAll(".nav-link").forEach(nav => nav.classList.remove("active"));
        const targetTab = this.getAttribute("href");
        document.querySelector(targetTab).classList.add("show", "active");
        this.classList.add("active");
    });
});

