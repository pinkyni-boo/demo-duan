namespace demo_duan.Models
{
    public class MoviesIndexViewModel
    {
        public List<Movie> Movies { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public string? Status { get; set; }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}