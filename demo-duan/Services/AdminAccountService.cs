using demo_duan.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace demo_duan.Services
{
    public static class AdminAccountService
    {
        public static async Task SeedAdminUser(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Tạo các roles nếu chưa tồn tại
            string[] roles = { "Admin", "Manager", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    Console.WriteLine($"✅ Đã tạo role {role}");
                }
            }

            // Thông tin tài khoản admin
            var adminEmail = "admin@movietheater.com";
            var adminPassword = "Admin@123"; // Đặt mật khẩu mạnh mẽ hơn

            // Kiểm tra nếu admin chưa tồn tại
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    PhoneNumber = "0123456789",
                    CreatedDate = DateTime.Now
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    // Gán quyền admin
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine($"✅ Đã tạo tài khoản admin {adminEmail} với mật khẩu {adminPassword}");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    Console.WriteLine($"❌ Không thể tạo tài khoản admin: {errors}");
                }
            }
            else
            {
                // Đảm bảo người dùng admin có quyền Admin
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine($"✅ Đã gán quyền Admin cho {adminEmail}");
                }
            }
        }
    }
}