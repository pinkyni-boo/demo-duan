using demo_duan.Models;
using Microsoft.EntityFrameworkCore;
using demo_duan.Data;

namespace demo_duan.Services
{
    public class MovieStatusUpdateService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private Timer? _timer;

        public MovieStatusUpdateService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(24));
            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var movies = context.Movies.ToList();
            var updated = false;

            foreach (var movie in movies)
            {
                var oldStatus = movie.Status;
                movie.UpdateStatusBasedOnReleaseDate();
                
                if (oldStatus != movie.Status)
                {
                    updated = true;
                }
            }

            if (updated)
            {
                context.SaveChanges();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}