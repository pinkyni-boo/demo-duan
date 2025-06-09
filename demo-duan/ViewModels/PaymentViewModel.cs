using System;
using System.Collections.Generic;
using demo_duan.Models;

namespace demo_duan.ViewModels
{
    public class PaymentViewModel
    {
        // Thông tin suất chiếu
        public int ShowtimeId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public DateTime ShowDate { get; set; }
        public TimeSpan ShowTime { get; set; }
        
        // Thông tin rạp và phòng
        public string TheaterName { get; set; } = string.Empty;
        public string CinemaName { get; set; } = string.Empty;
        
        // Thông tin ghế
        public List<string> SelectedSeats { get; set; } = new List<string>();
        public decimal TicketPrice { get; set; }
        
        // Phương thức thanh toán
        public int SelectedPaymentMethodId { get; set; }
        public List<PaymentMethod> AvailablePaymentMethods { get; set; } = new List<PaymentMethod>();
        
        // Tổng tiền
        public decimal TotalAmount => (TicketPrice * SelectedSeats.Count) + 
            (SelectedPaymentMethod?.TransactionFee ?? 0);
        
        // Thông tin khách hàng
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        
        // Helper property
        public PaymentMethod? SelectedPaymentMethod => 
            AvailablePaymentMethods.FirstOrDefault(p => p.Id == SelectedPaymentMethodId);
    }
}