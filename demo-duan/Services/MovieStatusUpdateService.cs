
using demo_duan.Data;
using Microsoft.EntityFrameworkCore;

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

                    await UpdateMovieStatuses(context);
                    
                    // Chạy mỗi giờ
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating movie statuses");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }

        private async Task UpdateMovieStatuses(ApplicationDbContext context)
        {
            var movies = await context.Movies.ToListAsync();
            bool hasChanges = false;

            foreach (var movie in movies)
            {
                var oldStatus = movie.Status;
                movie.UpdateStatusBasedOnReleaseDate();
                
                if (oldStatus != movie.Status)
                {
                    _logger.LogInformation($"Updated movie '{movie.Title}' status from '{oldStatus}' to '{movie.Status}'");
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                await context.SaveChangesAsync();
                _logger.LogInformation("Movie statuses updated successfully");
            }
        }
    }
}