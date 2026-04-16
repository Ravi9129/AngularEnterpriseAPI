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
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRoleAssignment> UserRoleAssignments { get; set; }
        // Dynamic permissions
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();

                // keep existing enum property mapping
                entity.Property(u => u.Role)
                    .HasConversion<int>()
                    .HasDefaultValue(Models.Enums.UserRole.USER);
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
            
            // Configure permission relationships (simple)
            modelBuilder.Entity<UserPermission>()
                .HasOne(up => up.User)
                .WithMany(u => u.UserPermissions)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserPermission>()
                .HasOne(up => up.Permission)
                .WithMany(p => p.UserPermissions)
                .HasForeignKey(up => up.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
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
                Role = Models.Enums.UserRole.ADMIN,
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
                Role = Models.Enums.UserRole.MANAGER,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            modelBuilder.Entity<User>().HasData(adminUser, regularUser, managerUser);

            // Seed base roles
            var roleAdmin = new Role { Id = 1, Name = "ADMIN", Description = "Administrator role", IsActive = true, CreatedAt = DateTime.UtcNow };
            var roleUser = new Role { Id = 2, Name = "USER", Description = "Default user role", IsActive = true, CreatedAt = DateTime.UtcNow };
            var roleManager = new Role { Id = 3, Name = "MANAGER", Description = "Manager role", IsActive = true, CreatedAt = DateTime.UtcNow };

            modelBuilder.Entity<Role>().HasData(roleAdmin, roleUser, roleManager);

            // Seed assignments linking seeded users to roles
            modelBuilder.Entity<UserRoleAssignment>().HasData(
                new UserRoleAssignment { Id = 1, UserId = 1, RoleId = 1, AssignedAt = DateTime.UtcNow }, // admin
                new UserRoleAssignment { Id = 2, UserId = 2, RoleId = 2, AssignedAt = DateTime.UtcNow }, // john.doe -> USER
                new UserRoleAssignment { Id = 3, UserId = 3, RoleId = 3, AssignedAt = DateTime.UtcNow }  // jane.manager -> MANAGER
            );
        }
    }
}
