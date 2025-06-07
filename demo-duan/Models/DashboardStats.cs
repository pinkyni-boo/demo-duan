namespace demo_duan.Models
{
    public class DashboardStats
    {
        public int TotalMovies { get; set; }
        public int TotalTheaters { get; set; }
        public int TotalCategories { get; set; }
        public int TotalShowtimes { get; set; }
        public int TotalTickets { get; set; }
        public int TodayShowtimesCount { get; set; }
        public int ComingSoonMovies { get; set; }
        public int ActiveMovies { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int TodayTicketsSold { get; set; }
        public int MonthlyTicketsSold { get; set; }
        
        // Recent data for dashboard
        public List<Movie> RecentMovies { get; set; } = new List<Movie>();
        public List<Showtime> UpcomingShowtimes { get; set; } = new List<Showtime>();
        public List<Ticket> RecentTickets { get; set; } = new List<Ticket>();
    }
}