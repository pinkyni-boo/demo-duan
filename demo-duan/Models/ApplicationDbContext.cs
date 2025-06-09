using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using demo_duan.Areas.Identity.Data;

namespace demo_duan.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Theater> Theaters { get; set; }
        public DbSet<Cinema> Cinemas { get; set; }
        public DbSet<Showtime> Showtimes { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Booking> Bookings { get; set; } // Thêm dòng này

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ApplicationUser configuration
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.FullName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.Gender)
                    .HasMaxLength(10);

                entity.Property(u => u.Address)
                    .HasMaxLength(500);

                entity.Property(u => u.Avatar)
                    .HasMaxLength(255);

                entity.Property(u => u.Notes)
                    .HasMaxLength(1000);

                entity.HasIndex(u => u.Email);
                entity.HasIndex(u => u.FullName);
            });

            // Movie configuration
            modelBuilder.Entity<Movie>(entity =>
            {
                entity.Property(m => m.Status)
                    .HasConversion<string>();

                entity.Property(m => m.Rating)
                    .HasColumnType("decimal(3,1)");

                entity.Property(m => m.Price)
                    .HasColumnType("decimal(18,2)");

                entity.HasIndex(m => m.Title);
                entity.HasIndex(m => m.ReleaseDate);
            });

            // Payment configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(p => p.Status)
                    .HasConversion<string>();

                entity.Property(p => p.Amount)
                    .HasColumnType("decimal(18,2)");

                entity.HasIndex(p => p.TransactionId);
            });

            // Invoice configuration
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.Property(i => i.TotalAmount)
                    .HasColumnType("decimal(18,2)");

                entity.Property(i => i.TaxAmount)
                    .HasColumnType("decimal(18,2)");

                entity.Property(i => i.SubTotal)
                    .HasColumnType("decimal(18,2)");

                entity.HasIndex(i => i.InvoiceNumber)
                    .IsUnique();

                entity.HasIndex(i => i.IssueDate);
            });

            // InvoiceItem configuration
            modelBuilder.Entity<InvoiceItem>(entity =>
            {
                entity.Property(ii => ii.UnitPrice)
                    .HasColumnType("decimal(18,2)");

                entity.Property(ii => ii.TotalPrice)
                    .HasColumnType("decimal(18,2)");
            });

            // Ticket configuration - Fixed to use Price instead of TotalPrice
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.Property(t => t.Price)
                    .HasColumnType("decimal(18,2)");

                entity.HasIndex(t => t.TicketCode)
                    .IsUnique();

                entity.HasIndex(t => t.BookingDate);
            });

            // Configure relationships
            ConfigureRelationships(modelBuilder);
        }

        private void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            // Category -> Movie
            modelBuilder.Entity<Movie>()
                .HasOne(m => m.Category)
                .WithMany(c => c.Movies)
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Theater -> Cinema
            modelBuilder.Entity<Cinema>()
                .HasOne(c => c.Theater)
                .WithMany(t => t.Cinemas)
                .HasForeignKey(c => c.TheaterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Movie -> Showtime
            modelBuilder.Entity<Showtime>()
                .HasOne(s => s.Movie)
                .WithMany(m => m.Showtimes)
                .HasForeignKey(s => s.MovieId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cinema -> Showtime
            modelBuilder.Entity<Showtime>()
                .HasOne(s => s.Cinema)
                .WithMany(c => c.Showtimes)
                .HasForeignKey(s => s.CinemaId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Ticket
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tickets)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Showtime -> Ticket
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Showtime)
                .WithMany(s => s.Tickets)
                .HasForeignKey(t => t.ShowtimeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Movie -> Ticket
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Movie)
                .WithMany(m => m.Tickets)
                .HasForeignKey(t => t.MovieId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Payment
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ticket -> Payment
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Ticket)
                .WithMany(t => t.Payments)
                .HasForeignKey(p => p.TicketId)
                .OnDelete(DeleteBehavior.Restrict);

            // PaymentMethod -> Payment
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.PaymentMethod)
                .WithMany(pm => pm.Payments)
                .HasForeignKey(p => p.PaymentMethodId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Invoice
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ticket -> Invoice
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Ticket)
                .WithMany(t => t.Invoices)
                .HasForeignKey(i => i.TicketId)
                .OnDelete(DeleteBehavior.Restrict);

            // Invoice -> InvoiceItem
            modelBuilder.Entity<InvoiceItem>()
                .HasOne(ii => ii.Invoice)
                .WithMany(i => i.InvoiceItems)
                .HasForeignKey(ii => ii.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}