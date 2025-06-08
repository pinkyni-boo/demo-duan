using Microsoft.AspNetCore.Identity.UI.Services;

namespace demo_duan.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // TODO: Implement email sending logic
            return Task.CompletedTask;
        }
    }
}