// filepath: d:\CODE\demo-duan\demo-duan\Data\DbChecker.cs
using demo_duan.Models;
using Microsoft.EntityFrameworkCore;

namespace demo_duan.Data
{
    public static class DbChecker
    {
        public static async Task CheckAndSeedAsync(ApplicationDbContext context, ILogger logger)
        {
            try
            {
                // Ensure database is created
                await context.Database.EnsureCreatedAsync();

                // Seed Categories
                if (!await context.Categories.AnyAsync())
                {
                    var categories = new[]
                    {
                        new Category { Name = "Hành động", Description = "Phim hành động" },
                        new Category { Name = "Hài kịch", Description = "Phim hài" },
                        new Category { Name = "Kinh dị", Description = "Phim kinh dị" },
                        new Category { Name = "Tình cảm", Description = "Phim tình cảm" },
                        new Category { Name = "Khoa học viễn tưởng", Description = "Phim sci-fi" },
                        new Category { Name = "Phiêu lưu", Description = "Phim phiêu lưu" }
                    };

                    context.Categories.AddRange(categories);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Categories seeded successfully");
                }

                // Seed Theaters
                if (!await context.Theaters.AnyAsync())
                {
                    var theaters = new[]
                    {
                        new Theater 
                        { 
                            Name = "Rạp Galaxy 1", 
                            Capacity = 120, 
                            Location = "Tầng 1",
                            Address = "123 Nguyễn Văn Cừ, Quận 1, TP.HCM",
                            PhoneNumber = "028-3825-1234",
                            Email = "galaxy1@cinema.com"
                        },
                        new Theater 
                        { 
                            Name = "Rạp Galaxy 2", 
                            Capacity = 150, 
                            Location = "Tầng 2",
                            Address = "456 Lê Lợi, Quận 1, TP.HCM",
                            PhoneNumber = "028-3825-5678",
                            Email = "galaxy2@cinema.com"
                        },
                        new Theater 
                        { 
                            Name = "Rạp Galaxy 3", 
                            Capacity = 100, 
                            Location = "Tầng 3",
                            Address = "789 Trần Hưng Đạo, Quận 5, TP.HCM",
                            PhoneNumber = "028-3825-9012",
                            Email = "galaxy3@cinema.com"
                        },
                        new Theater 
                        { 
                            Name = "Rạp IMAX Premium", 
                            Capacity = 200, 
                            Location = "Tầng 4",
                            Address = "321 Hai Bà Trưng, Quận 3, TP.HCM",
                            PhoneNumber = "028-3825-3456",
                            Email = "imax@cinema.com"
                        }
                    };

                    context.Theaters.AddRange(theaters);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Theaters seeded successfully");
                }

                // Seed Payment Methods
                if (!await context.PaymentMethods.AnyAsync())
                {
                    var paymentMethods = new[]
                    {
                        new PaymentMethod { Name = "Tiền mặt", Description = "Thanh toán bằng tiền mặt tại quầy" },
                        new PaymentMethod { Name = "Thẻ tín dụng", Description = "Thanh toán bằng thẻ Visa/Mastercard" },
                        new PaymentMethod { Name = "Ví điện tử", Description = "Thanh toán qua MoMo, ZaloPay" },
                        new PaymentMethod { Name = "Chuyển khoản", Description = "Chuyển khoản ngân hàng" }
                    };

                    context.PaymentMethods.AddRange(paymentMethods);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Payment methods seeded successfully");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while seeding database");
                throw;
            }
        }
    }
}