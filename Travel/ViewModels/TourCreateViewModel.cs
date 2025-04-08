using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
namespace Travel.ViewModels
{
    public class TourCreateViewModel
    {
        [Required(ErrorMessage = "Tên tour không được để trống")]
        public string TourName { get; set; }

        [Required(ErrorMessage = "Điểm đến bắt buộc")]
        public int? DestinationId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc bắt buộc")]
        [DataType(DataType.Date)]
        [DateGreaterThan("StartDate", ErrorMessage = "Ngày kết thúc phải sau ngày bắt đầu")]
        public DateTime EndDate { get; set; }

    }
}
