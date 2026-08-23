using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WorkPackageService.Context;
using WorkPackageService.Profiles;

namespace WorkPackageService.Tests.TestHelpers
{
    public static class DbContextFactory
    {
        // Svaki poziv dobija sopstvenu in-memory bazu (jedinstveno ime), da testovi ne dele stanje.
        public static WorkPackageServiceContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<WorkPackageServiceContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new WorkPackageServiceContext(options);
        }

        // Realan AutoMapper iz stvarnih Profile klasa - ne mokovan, testira i samo mapiranje.
        public static IMapper CreateMapper()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<WorkPackageProfile>();
                cfg.AddProfile<BacklogProfile>();
                cfg.AddProfile<TaskProfile>();
                cfg.AddProfile<DependencyProfile>();
                cfg.AddProfile<CommentProfile>();
            });

            return configuration.CreateMapper();
        }
    }
}
