document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("booking-form");
    const bookingSummary = document.getElementById("booking-summary");

    if (form) {
        form.addEventListener("submit", function (event) {
            event.preventDefault();

            const tour = document.getElementById("tour").value;
            const name = document.getElementById("name").value;
            const email = document.getElementById("email").value;
            const phone = document.getElementById("phone").value;

            const phoneError = document.getElementById("phone-error");
            if (!/^\d+$/.test(phone)) {
                phoneError.textContent = "Vui lòng không nhập chữ, kí tự đặc biệt, khoảng trống.";
                return;
            } else {
                phoneError.textContent = "";
            }

            const emailError = document.getElementById("email-error");
            const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            const invalidChars = /[ /!*,:;]/;
            if (invalidChars.test(email) || !emailPattern.test(email)) {
                emailError.textContent = "Email không hợp lệ. Vui lòng kiểm tra lại.";
                return;
            } else {
                emailError.textContent = "";
            }

            const tourName = document.getElementById("tour").options[document.getElementById("tour").selectedIndex].text;
            const tourImage = getTourImage(tour);

            document.getElementById("tour-name").textContent = tourName;
            document.getElementById("summary-name").textContent = name;
            document.getElementById("summary-email").textContent = email;
            document.getElementById("summary-phone").textContent = phone;
            document.getElementById("tour-image").src = tourImage;

            bookingSummary.style.display = "block";
        });
    }

    function getTourImage(tourValue) {
        switch (tourValue) {
            case "1":
                return "/images/dalat.jpg";
            case "2":
                return "/images/nhatrang.jpg";
            case "3":
                return "/images/halong.jpg";
            default:
                return "";
        }
    }
});