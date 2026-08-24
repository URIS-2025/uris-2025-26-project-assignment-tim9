using Microsoft.EntityFrameworkCore;
using AuthService.Models;

namespace AuthService.Context
{
    public class AuthContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public AuthContext(
            DbContextOptions<AuthContext> options,
            IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        // Tabele
        public DbSet<AuthSession> AuthSessions { get; set; }

        // Konekcija sa bazom
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("AuthDB");
                optionsBuilder.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString)
                );
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AuthSession>().HasKey(s => s.AuthId);
        }
    }
}
