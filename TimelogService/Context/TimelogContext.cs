using Microsoft.EntityFrameworkCore;
using TimelogService.Models;

namespace TimelogService.Context
{
    public class TimelogContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public TimelogContext(DbContextOptions<TimelogContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        //tabela
        public DbSet<Timelog> Timelogs { get; set; }

        //konekcija sa bazom
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("TimelogDB");
                optionsBuilder.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString)
                );
            }
        }

        //inicijalni podaci
        //
        // Projects/Tasks/Users referenced here are seeded by ProjectService,
        // WorkPackageService and UserService - see WorkPackageServiceContext for the
        // canonical Task Id values these must line up with.
        protected override void OnModelCreating(ModelBuilder builder)
        {
            var project1 = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
            var project3 = Guid.Parse("a3000000-0000-0000-0000-000000000003");

            var task1 = Guid.Parse("80000001-0000-0000-0000-000000000001");
            var task2 = Guid.Parse("80000001-0000-0000-0000-000000000002");
            var task3 = Guid.Parse("80000001-0000-0000-0000-000000000003");
            var task4 = Guid.Parse("80000003-0000-0000-0000-000000000001");

            var userPm = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var userMember = Guid.Parse("33333333-3333-3333-3333-333333333333");

            builder.Entity<Timelog>().HasData(
                new Timelog
                {
                    Id = Guid.Parse("90000001-0000-0000-0000-000000000001"),
                    ProjectId = project1,
                    TaskId = task1,
                    HoursSpent = 6,
                    Date = new DateTime(2026, 6, 5),
                    LoggedByUserId = userMember
                },
                new Timelog
                {
                    Id = Guid.Parse("90000001-0000-0000-0000-000000000002"),
                    ProjectId = project1,
                    TaskId = task2,
                    HoursSpent = 5.5,
                    Date = new DateTime(2026, 6, 20),
                    LoggedByUserId = userMember
                },
                new Timelog
                {
                    Id = Guid.Parse("90000001-0000-0000-0000-000000000003"),
                    ProjectId = project1,
                    TaskId = task3,
                    HoursSpent = 4,
                    Date = new DateTime(2026, 8, 25),
                    LoggedByUserId = userMember
                },
                new Timelog
                {
                    Id = Guid.Parse("90000003-0000-0000-0000-000000000001"),
                    ProjectId = project3,
                    TaskId = task4,
                    HoursSpent = 7,
                    Date = new DateTime(2025, 6, 2),
                    LoggedByUserId = userPm
                }
            );
        }
    }
}
