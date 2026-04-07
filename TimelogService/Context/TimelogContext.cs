using System.Collections.Generic;
using System.Reflection.Emit;
using TimelogService.Models;
using Microsoft.EntityFrameworkCore;

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
            var connectionString = _configuration.GetConnectionString("TimelogDB");
            optionsBuilder.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)
            );
        }

        //inicijalni podaci
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Timelog>().HasData(new Timelog
            {
                Id = Guid.Parse("7a411c13-a195-48f7-8dbd-67596c3974c0"),
                ProjectId = Guid.Parse("044f3de0-a9dd-4c2e-b745-89976a1b2a36"),
                WorkPackageId = Guid.Parse("21ad52f8-0281-4241-98b0-481566d25e4f"),
                HoursSpent = 4.5,
                Date = new DateTime(2026, 4, 7)
            });
        }
    }
}
