using System.Collections.Generic;

namespace demo_duan.ViewModels
{
    public class CinemaShowtimeViewModel
    {
        public int CinemaId { get; set; }
        public string CinemaName { get; set; }
        public string CinemaType { get; set; }
        public List<ShowtimeItemViewModel> Showtimes { get; set; } = new List<ShowtimeItemViewModel>();
    }

}