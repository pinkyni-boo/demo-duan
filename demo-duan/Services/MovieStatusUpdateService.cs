using Microsoft.EntityFrameworkCore;
using demo_duan.Data;

namespace demo_duan.Services
{
    public class MovieStatusUpdateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MovieStatusUpdateService> _logger;

        public MovieStatusUpdateService(IServiceProvider serviceProvider, ILogger<MovieStatusUpdateService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var movies = await context.Movies
                        .Where(m => m.IsActive)
                        .ToListAsync(stoppingToken);

                    var updatedCount = 0;
                    foreach (var movie in movies)
                    {
                        var oldStatus = movie.Status;
                        movie.UpdateStatusBasedOnReleaseDate();
                        
                        if (oldStatus != movie.Status)
                        {
                            context.Update(movie);
                            updatedCount++;
                            _logger.LogInformation($"Updated movie {movie.Title} status from {oldStatus} to {movie.Status}");
                        }
                    }

                    if (updatedCount > 0)
                    {
                        await context.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation($"Updated status for {updatedCount} movies");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating movie statuses");
                }

                // Chạy mỗi giờ
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}