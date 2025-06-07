// filepath: d:\CODE\demo-duan\demo-duan\Data\DbChecker.cs
using demo_duan.Models;
using Microsoft.EntityFrameworkCore;

namespace demo_duan.Data
{
    public static class DbChecker
    {
        public static async Task EnsureDatabaseSeeded(ApplicationDbContext context)
        {
            await context.Database.EnsureCreatedAsync();

            // Seed Categories
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Hành động", Description = "Phim hành động" },
                    new Category { Name = "Kinh dị", Description = "Phim kinh dị" },
                    new Category { Name = "Hài hước", Description = "Phim hài hước" },
                    new Category { Name = "Lãng mạn", Description = "Phim lãng mạn" },
                    new Category { Name = "Khoa học viễn tưởng", Description = "Phim khoa học viễn tưởng" }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // Seed Movies
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
                        Cast = "Vin Diesel, Jason Momoa, Michelle Rodriguez"
                    }
                };

                foreach (var movie in movies)
                {
                    movie.UpdateStatusBasedOnReleaseDate();
                }

                await context.Movies.AddRangeAsync(movies);
                await context.SaveChangesAsync();
            }

            // Seed Theaters
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
                    },
                    new Theater
                    {
                        Name = "Galaxy Nguyễn Du",
                        Address = "Nguyễn Du, Quận 1, TP.HCM", 
                        Phone = "0901234568",
                        Email = "galaxy.nguyendu@cinema.com",
                        City = "TP.HCM",
                        IsActive = true
                    },
                    new Theater
                    {
                        Name = "Lotte Cinema",
                        Address = "Lotte Center, Quận 1, TP.HCM",
                        Phone = "0901234569",
                        Email = "lotte.center@cinema.com",
                        City = "TP.HCM",
                        IsActive = true
                    },
                    new Theater
                    {
                        Name = "BHD Star Bitexco",
                        Address = "Bitexco Financial Tower, Quận 1, TP.HCM",
                        Phone = "0901234570",
                        Email = "bhd.bitexco@cinema.com",
                        City = "TP.HCM",
                        IsActive = true
                    }
                };

                await context.Theaters.AddRangeAsync(theaters);
                await context.SaveChangesAsync();
            }

            // Seed Payment Methods
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
                    },
                    new PaymentMethod
                    {
                        Name = "MoMo",
                        Description = "Thanh toán qua ví MoMo",
                        Icon = "/images/payment/momo.png",
                        IsActive = true,
                        TransactionFee = 2.0m,
                        DisplayOrder = 3
                    },
                    new PaymentMethod
                    {
                        Name = "ZaloPay",
                        Description = "Thanh toán qua ZaloPay",
                        Icon = "/images/payment/zalopay.png",
                        IsActive = true,
                        TransactionFee = 1.8m,
                        DisplayOrder = 4
                    }
                };

                await context.PaymentMethods.AddRangeAsync(paymentMethods);
                await context.SaveChangesAsync();
            }

            // Seed sample Payments (nếu cần)
            if (!await context.Payments.AnyAsync())
            {
                var firstTicket = await context.Tickets.FirstOrDefaultAsync();
                var cashMethod = await context.PaymentMethods.FirstOrDefaultAsync(pm => pm.Name == "Tiền mặt");
                
                if (firstTicket != null && cashMethod != null)
                {
                    var samplePayment = new Payment
                    {
                        TicketId = firstTicket.Id,
                        PaymentMethodId = cashMethod.Id,
                        UserId = firstTicket.UserId,
                        Amount = firstTicket.TotalPrice,
                        Status = PaymentStatus.Completed,
                        PaymentDate = DateTime.Now,
                        TransactionId = "CASH_" + DateTime.Now.Ticks,
                        ProcessedDate = DateTime.Now
                    };

                    await context.Payments.AddAsync(samplePayment);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}