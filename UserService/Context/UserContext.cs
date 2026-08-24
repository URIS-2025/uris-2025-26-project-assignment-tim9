using Microsoft.EntityFrameworkCore;
using UserService.Models;
using UserService.Models.Enums;
using UserService.Services;

namespace UserService.Context
{
    public class UserContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public UserContext(
            DbContextOptions<UserContext> options,
            IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        // Tabele
        public DbSet<User> Users { get; set; }
        public DbSet<UserActivityLog> UserActivityLogs { get; set; }

        // Konekcija sa bazom
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("UserDB");
                optionsBuilder.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString)
                );
            }
        }

        // Inicijalni podaci
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserActivityLog>().HasKey(l => l.LogId);

            var passwordService = new PasswordService();
            var (hash, salt) = passwordService.HashPassword("password123");

            builder.Entity<User>().HasData(
                new User
                {
                    UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "Admin Administrator",
                    Username = "admin",
                    Email = "admin@example.com",
                    ContactInfo = "+381600000001",
                    Role = UserRole.Admin,
                    IsActive = true,
                    PasswordHash = hash,
                    Salt = salt,
                    CreatedAt = DateTime.Parse("2025-01-01T00:00:00")
                },
                new User
                {
                    UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Petar Projektni",
                    Username = "pm",
                    Email = "pm@example.com",
                    ContactInfo = "+381600000002",
                    Role = UserRole.ProjectManager,
                    IsActive = true,
                    PasswordHash = hash,
                    Salt = salt,
                    CreatedAt = DateTime.Parse("2025-01-01T00:00:00")
                },
                new User
                {
                    UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "Marko Član",
                    Username = "member",
                    Email = "member@example.com",
                    ContactInfo = "+381600000003",
                    Role = UserRole.TeamMember,
                    IsActive = true,
                    PasswordHash = hash,
                    Salt = salt,
                    CreatedAt = DateTime.Parse("2025-01-01T00:00:00")
                },
                new User
                {
                    UserId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Name = "Klijent Test",
                    Username = "client",
                    Email = "client@example.com",
                    ContactInfo = "+381600000004",
                    Role = UserRole.Client,
                    IsActive = true,
                    PasswordHash = hash,
                    Salt = salt,
                    CreatedAt = DateTime.Parse("2025-01-01T00:00:00")
                }
            );
        }
    }
}
