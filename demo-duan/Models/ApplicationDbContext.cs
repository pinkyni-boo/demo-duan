using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using demo_duan.Models;

namespace demo_duan.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Theater> Theaters { get; set; }
        public DbSet<Showtime> Showtimes { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Movie relationships
            modelBuilder.Entity<Movie>()
                .HasOne(m => m.Category)
                .WithMany(c => c.Movies)
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Ticket - Movie relationship
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Movie)
                .WithMany(m => m.Tickets)
                .HasForeignKey(t => t.MovieId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ticket - Showtime relationship
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Showtime)
                .WithMany(s => s.Tickets)
                .HasForeignKey(t => t.ShowtimeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Showtime relationships
            modelBuilder.Entity<Showtime>()
                .HasOne(s => s.Movie)
                .WithMany(m => m.Showtimes)
                .HasForeignKey(s => s.MovieId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Showtime>()
                .HasOne(s => s.Theater)
                .WithMany(t => t.Showtimes)
                .HasForeignKey(s => s.TheaterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Decimal precision
            modelBuilder.Entity<Movie>()
                .Property(m => m.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Ticket>()
                .Property(t => t.Price)
                .HasColumnType("decimal(18,2)");

            // Unique constraint for seat booking
            modelBuilder.Entity<Ticket>()
                .HasIndex(t => new { t.ShowtimeId, t.SeatNumber })
                .IsUnique()
                .HasDatabaseName("IX_Ticket_Showtime_Seat");
        }
    }
}
