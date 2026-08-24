using AutoMapper;
using Microsoft.EntityFrameworkCore;
using IntegrationService.Context;
using IntegrationService.Profiles;

namespace IntegrationService.Tests.TestHelpers
{
    public static class DbContextFactory
    {
        // Svaki poziv dobija sopstvenu in-memory bazu (jedinstveno ime), da testovi ne dele stanje.
        public static IntegrationServiceContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<IntegrationServiceContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new IntegrationServiceContext(options);
        }

        // Realan AutoMapper iz stvarne Profile klase - ne mokovan, testira i samo mapiranje.
        public static IMapper CreateMapper()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<IntegrationProfile>();
            });

            return configuration.CreateMapper();
        }
    }
}
