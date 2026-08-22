using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NotificationService.Context
{
    // Koristi se samo od strane EF Core alata (migrations) u design-time-u, kada nema zive
    // konekcije na MySQL da bi ServerVersion.AutoDetect (koji se koristi u Program.cs) mogao
    // da se izvrsi. Runtime ponasanje aplikacije ostaje nepromenjeno.
    public class NotificationServiceContextFactory : IDesignTimeDbContextFactory<NotificationServiceContext>
    {
        public NotificationServiceContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NotificationServiceContext>();
            optionsBuilder.UseMySql(
                "Server=localhost;Port=3306;Database=NotificationServiceDB;User=root;Password=root;",
                new MySqlServerVersion(new Version(8, 0, 35)));

            return new NotificationServiceContext(optionsBuilder.Options);
        }
    }
}
