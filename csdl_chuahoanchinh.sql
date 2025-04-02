INSERT INTO Destinations (Name, Country, City, Description, ImageUrl, Latitude, Longitude, IsPopular)
VALUES
(N'Vịnh Hạ Long', N'Việt Nam', N'Quảng Ninh', N'Di sản thiên nhiên thế giới với hàng nghìn đảo đá vôi.', 'https://example.com/halong.jpg', 20.910051, 107.183902, 1),
(N'Phố cổ Hội An', N'Việt Nam', N'Quảng Nam', N'Di sản văn hóa thế giới với kiến trúc cổ kính.', 'https://example.com/hoian.jpg', 15.880058, 108.338047, 1),
(N'Cố đô Huế', N'Việt Nam', N'Thừa Thiên Huế', N'Kinh thành cổ kính của triều Nguyễn.', 'https://example.com/hue.jpg', 16.463713, 107.590866, 1),
(N'Thánh địa Mỹ Sơn', N'Việt Nam', N'Quảng Nam', N'Di sản văn hóa thế giới của người Chăm.', 'https://example.com/myson.jpg', 15.775396, 108.126401, 0),
(N'Động Phong Nha', N'Việt Nam', N'Quảng Bình', N'Hang động kỳ vĩ thuộc vườn quốc gia Phong Nha - Kẻ Bàng.', 'https://example.com/phongnha.jpg', 17.558756, 106.286263, 1),
(N'Bà Nà Hills', N'Việt Nam', N'Đà Nẵng', N'Khu du lịch nổi tiếng với Cầu Vàng.', 'https://example.com/banahills.jpg', 15.995008, 107.997879, 1),
(N'Đà Lạt', N'Việt Nam', N'Lâm Đồng', N'Thành phố ngàn hoa với khí hậu mát mẻ.', 'https://example.com/dalat.jpg', 11.940419, 108.458313, 1),
(N'Sapa', N'Việt Nam', N'Lào Cai', N'Thị trấn vùng cao với ruộng bậc thang và Fansipan.', 'https://example.com/sapa.jpg', 22.336524, 103.843057, 1),
(N'Đảo Phú Quốc', N'Việt Nam', N'Kiên Giang', N'Hòn đảo thiên đường với biển xanh và cát trắng.', 'https://example.com/phuquoc.jpg', 10.227013, 103.963675, 1),
(N'Côn Đảo', N'Việt Nam', N'Bà Rịa - Vũng Tàu', N'Hòn đảo lịch sử với thiên nhiên hoang sơ.', 'https://example.com/condao.jpg', 8.680262, 106.609139, 0);

	INSERT INTO Tours(
		DestinationId, TourName, Description, Price, Duration, StartDate, EndDate, 
		AvailableSeats, TourGuideId, ImageUrl, TourType, TourStatus
	)
	VALUES 
		(1, N'Tour Đà Lạt ', N'Khám phá thành phố ngàn hoa với đồi thông bạt ngàn.', 
     2500000.00, 3, '2025-04-15 08:00:00', '2025-04-17 18:00:00', 
     20, '2ddd5220-3336-417e-b09a-2595fefb28ce', '/images/dalat.jpg', 'Group', 'Upcoming'),
    (2, N'Tour Nha Trang', N'Thưởng thức bãi biển xanh, cát trắng và nắng vàng tại Nha Trang.', 
     3000000.00, 4, '2025-05-01 09:00:00', '2025-05-04 17:00:00', 
     15, '528bf958-919a-4e78-9e7b-174d1b8a9874', '/images/nhatrang.jpg', 'Private', 'Upcoming'),
    (3, N'Tour Hạ Long', N'Đắm mình vào vẻ đẹp hoang sơ và thiên đường biển tại Hạ Long.', 
     3500000.00, 2, '2025-06-10 07:00:00', '2025-06-11 20:00:00', 
     25, 'fe6db55f-c1d1-4eec-9a6e-6b941966914c', '/images/halong.jpg', 'Group', 'Upcoming');

	 INSERT INTO Vouchers(Code, Description, DiscountAmount, DiscountPercentage, MinimumBookingValue, MaxDiscountAmount, ExpiryDate, UsageLimit, UsageCount, IsActive, CreatedAt)
VALUES
('DISCOUNT10', '10% off on all tours', 0.00, 10.00, 100.00, 50.00, '2025-12-31', 100, 10, 1, '2025-04-01'),
('FLAT50', 'Flat $50 discount on bookings over $500', 50.00, NULL, 500.00, NULL, '2025-06-30', 50, 5, 1, '2025-04-01');

	 INSERT INTO Bookings (UserId, TourId, NumberOfAdults, NumberOfChildren, TotalPrice, BookingDate, Status, PaymentStatus, VoucherID, StartDate, DiscountAmountApplied, DiscountPercentageApplied)
VALUES
( '2ddd5220-3336-417e-b09a-2595fefb28ce', 8, 2, 1, 500.00, '2025-04-01', 'Confirmed', 'Paid', 1, '2025-05-01', NULL, NULL),
( '528bf958-919a-4e78-9e7b-174d1b8a9874', 9, 1, 0, 300.00, '2025-04-02', 'Pending', 'Pending', 2, '2025-06-10', 50.00, 10.0),
( 'fe6db55f-c1d1-4eec-9a6e-6b941966914c', 10, 3, 2, 1200.00, '2025-04-03', 'Cancelled', 'Refunded', 1, '2025-07-15', NULL, NULL);