using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using UserService.Profiles;
using Xunit;

namespace UserService.Tests
{
    public class UserProfileTests
    {
        [Fact]
        public void AutoMapperConfiguration_IsValid()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<UserProfile>(), NullLoggerFactory.Instance);
            config.AssertConfigurationIsValid();
        }
    }
}
