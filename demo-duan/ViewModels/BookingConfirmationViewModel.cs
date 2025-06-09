using System;
using System.Collections.Generic;
using demo_duan.Models;

namespace demo_duan.ViewModels
{
    public class BookingConfirmationViewModel
    {
        // Thông tin đơn hàng
        public string BookingCode { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        
        // Thông tin phim
        public string MovieTitle { get; set; } = string.Empty;
        public string MovieRating { get; set; } = string.Empty;
        public int MovieDuration { get; set; }
        
        // Thông tin suất chiếu
        public DateTime ShowDate { get; set; }
        public TimeSpan ShowTime { get; set; }
        
        // Thông tin rạp và phòng
        public string TheaterName { get; set; } = string.Empty;
        public string CinemaName { get; set; } = string.Empty;
        public string CinemaType { get; set; } = string.Empty;
        
        // Thông tin ghế và giá
        public List<string> SelectedSeats { get; set; } = new List<string>();
        public decimal TicketPrice { get; set; }
        public decimal TransactionFee { get; set; }
        public decimal TotalAmount { get; set; }
        
        // Thông tin khách hàng
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        
        // Helper properties
        public bool IsPaymentCompleted => PaymentStatus == "Completed";
        public bool NeedsPaymentAtCounter => PaymentStatus == "Pending" && PaymentMethod.Contains("Trực tiếp");
    }
}