// filepath: d:\CODE\demo-duan\demo-duan\Data\DbChecker.cs
using demo_duan.Models;
using Microsoft.EntityFrameworkCore;

namespace demo_duan.Data
{
    public static class DbChecker
    {
        public static async Task SeedDataAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                // Ensure database is created
                await context.Database.EnsureCreatedAsync();

                // Seed Categories
                await SeedCategoriesAsync(context);

                // Seed Payment Methods
                await SeedPaymentMethodsAsync(context);

                // Seed Theaters
                await SeedTheatersAsync(context);

                // Seed Movies
                await SeedMoviesAsync(context);

                await context.SaveChangesAsync();
                Console.WriteLine("✅ Database seeded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error seeding database: {ex.Message}");
                throw;
            }
        }

        private static async Task SeedCategoriesAsync(ApplicationDbContext context)
        {
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Hành động", Description = "Phim hành động", IsActive = true },
                    new Category { Name = "Kinh dị", Description = "Phim kinh dị", IsActive = true },
                    new Category { Name = "Hài hước", Description = "Phim hài hước", IsActive = true },
                    new Category { Name = "Lãng mạn", Description = "Phim lãng mạn", IsActive = true },
                    new Category { Name = "Khoa học viễn tưởng", Description = "Phim khoa học viễn tưởng", IsActive = true }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedMoviesAsync(ApplicationDbContext context)
        {
            if (!await context.Movies.AnyAsync())
            {
                var actionCategory = await context.Categories.FirstAsync(c => c.Name == "Hành động");
                var movies = new List<Movie>
                {
                    new Movie
                    {
                        Title = "Fast & Furious X",
                        Description = "Phim hành động tốc độ",
                        Duration = 120,
                        Price = 120000,
                        ReleaseDate = DateTime.Today.AddDays(-30),
                        CategoryId = actionCategory.Id,
                        Img = "/images/movies/fast-furious.jpg",
                        IsActive = true,
                        Rating = 8.5m,
                        Language = "Tiếng Anh - Phụ đề Việt",
                        Director = "Louis Leterrier",
                        Cast = "Vin Diesel, Jason Momoa, Michelle Rodriguez",
                        Status = MovieStatus.NowShowing
                    }
                };

                await context.Movies.AddRangeAsync(movies);
                await context.SaveChangesAsync();
                
                // Update movie status
                foreach (var movie in movies)
                {
                    movie.UpdateStatusBasedOnReleaseDate();
                }
                
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedTheatersAsync(ApplicationDbContext context)
        {
            if (!await context.Theaters.AnyAsync())
            {
                var theaters = new List<Theater>
                {
                    new Theater
                    {
                        Name = "CGV Vincom",
                        Address = "Vincom Center, Quận 1, TP.HCM",
                        Phone = "0901234567",
                        Email = "cgv.vincom@cinema.com",
                        City = "TP.HCM",
                        IsActive = true
                    }
                };

                await context.Theaters.AddRangeAsync(theaters);
                await context.SaveChangesAsync();

                // Seed Cinemas for each theater
                var theater = theaters.First();
                var cinemas = new List<Cinema>
                {
                    new Cinema
                    {
                        Name = "Phòng chiếu 1",
                        TheaterId = theater.Id,
                        TotalSeats = 100,
                        Rows = 10,
                        SeatsPerRow = 10,
                        Type = "2D",
                        IsActive = true
                    },
                    new Cinema
                    {
                        Name = "Phòng chiếu 2", 
                        TheaterId = theater.Id,
                        TotalSeats = 80,
                        Rows = 8,
                        SeatsPerRow = 10,
                        Type = "3D",
                        IsActive = true
                    }
                };

                await context.Cinemas.AddRangeAsync(cinemas);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedPaymentMethodsAsync(ApplicationDbContext context)
        {
            if (!await context.PaymentMethods.AnyAsync())
            {
                var paymentMethods = new List<PaymentMethod>
                {
                    new PaymentMethod
                    {
                        Name = "Tiền mặt",
                        Description = "Thanh toán bằng tiền mặt tại quầy",
                        Icon = "/images/payment/cash.png",
                        IsActive = true,
                        TransactionFee = 0,
                        DisplayOrder = 1
                    },
                    new PaymentMethod
                    {
                        Name = "VNPay",
                        Description = "Thanh toán qua ví điện tử VNPay",
                        Icon = "/images/payment/vnpay.png",
                        IsActive = true,
                        TransactionFee = 1.5m,
                        DisplayOrder = 2
                    }
                };

                await context.PaymentMethods.AddRangeAsync(paymentMethods);
                await context.SaveChangesAsync();
            }
        }
    }
}