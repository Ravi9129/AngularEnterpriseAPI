using AngularEnterpriseAPI.Models.Entities;
using AngularEnterpriseAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace AngularEnterpriseAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();

                entity.Property(u => u.Role)
                    .HasConversion<int>()
                    .HasDefaultValue(UserRole.USER);
            });

            // RefreshToken configuration
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasIndex(rt => rt.Token).IsUnique();

                entity.HasOne(rt => rt.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Create admin user
            var adminUser = new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@enterprise.com",
                FirstName = "System",
                LastName = "Administrator",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.ADMIN,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var regularUser = new User
            {
                Id = 2,
                Username = "john.doe",
                Email = "john.doe@example.com",
                FirstName = "John",
                LastName = "Doe",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                Role = UserRole.USER,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var managerUser = new User
            {
                Id = 3,
                Username = "jane.manager",
                Email = "jane.manager@example.com",
                FirstName = "Jane",
                LastName = "Manager",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager@123"),
                Role = UserRole.MANAGER,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            modelBuilder.Entity<User>().HasData(adminUser, regularUser, managerUser);
        }
    }
}
