using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using demo_duan.Models;

namespace demo_duan.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Theater> Theaters { get; set; }
        public DbSet<Showtime> Showtimes { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Movie configuration
            modelBuilder.Entity<Movie>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                
                entity.HasOne(e => e.Category)
                      .WithMany(c => c.Movies)
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // Showtime configuration
            modelBuilder.Entity<Showtime>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(e => e.Movie)
                      .WithMany(m => m.Showtimes)
                      .HasForeignKey(e => e.MovieId)
                      .OnDelete(DeleteBehavior.NoAction);
                      
                entity.HasOne(e => e.Theater)
                      .WithMany(t => t.Showtimes)
                      .HasForeignKey(e => e.TheaterId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // Ticket configuration
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MovieTitle).HasMaxLength(200);
                entity.Property(e => e.CinemaName).HasMaxLength(200);
                entity.Property(e => e.Seat).HasMaxLength(10);
                entity.Property(e => e.UserEmail).HasMaxLength(256);
                entity.Property(e => e.UserName).HasMaxLength(256);
                entity.Property(e => e.UserRole).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(20);

                entity.HasOne(e => e.Movie)
                      .WithMany(m => m.Tickets)
                      .HasForeignKey(e => e.MovieId)
                      .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.Showtime)
                      .WithMany(s => s.Tickets)
                      .HasForeignKey(e => e.ShowtimeId)
                      .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Tickets)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}
