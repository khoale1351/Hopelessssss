using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Travel.Models;

namespace Travel.ViewModels
{
    public class BookingViewModel 
    {
        public int TourId { get; set; }
        public string TourName { get; set; }
        public decimal TourPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Number of adults must be greater than or equal to 0")]
        public int NumberOfAdults { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Number of children must be greater than or equal to 0")]
        public int NumberOfChildren { get; set; }

        public int? VoucherID { get; set; }
        public decimal? DiscountAmountApplied { get; set; }
        public decimal? DiscountPercentageApplied { get; set; }
        public List<Voucher> AvailableVouchers { get; set; } = new List<Voucher>();
    }
}
