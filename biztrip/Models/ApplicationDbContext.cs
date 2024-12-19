using biztrip.Models;
using biztrip.Request;
using Microsoft.EntityFrameworkCore;

namespace RegisterForTour.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Khai báo các DbSet cho các thực thể
        public DbSet<User> Users { get; set; }
        public DbSet<Stage> Stages { get; set; }
        public DbSet<Registration> Registration { get; set; }

        public DbSet<RegistrationRespone> RegistrationResponse { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RegistrationRespone>().HasNoKey(); // Mark it as keyless
        }
    }
}
