using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using AuthService.Context;
using AuthService.Data;
using AuthService.Profiles;
using Xunit;

namespace AuthService.Tests
{
    public class AuthRepositoryTests
    {
        private static AuthContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AuthContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var configuration = new ConfigurationBuilder().Build();
            return new AuthContext(options, configuration);
        }

        private static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<AuthProfile>(), NullLoggerFactory.Instance);
            return config.CreateMapper();
        }

        [Fact]
        public void CreateSession_PersistsSessionWithGivenData()
        {
            var context = CreateContext();
            var repository = new AuthRepository(context, CreateMapper());
            var userId = Guid.NewGuid();

            var session = repository.CreateSession(userId, "admin", "Admin", "refresh-token-123", DateTime.UtcNow.AddDays(7));

            Assert.NotEqual(Guid.Empty, session.AuthId);
            Assert.Equal(userId, session.UserId);
            Assert.False(session.IsRevoked);
            Assert.Single(context.AuthSessions);
        }

        [Fact]
        public void GetByRefreshToken_UnknownToken_ReturnsNull()
        {
            var context = CreateContext();
            var repository = new AuthRepository(context, CreateMapper());

            var result = repository.GetByRefreshToken("does-not-exist");

            Assert.Null(result);
        }

        [Fact]
        public void RevokeSession_ExistingToken_MarksAsRevoked()
        {
            var context = CreateContext();
            var repository = new AuthRepository(context, CreateMapper());
            var session = repository.CreateSession(Guid.NewGuid(), "admin", "Admin", "token-abc", DateTime.UtcNow.AddDays(7));

            var success = repository.RevokeSession("token-abc");

            Assert.True(success);
            Assert.True(context.AuthSessions.First(s => s.AuthId == session.AuthId).IsRevoked);
        }

        [Fact]
        public void RevokeSession_UnknownToken_ReturnsFalse()
        {
            var context = CreateContext();
            var repository = new AuthRepository(context, CreateMapper());

            var success = repository.RevokeSession("does-not-exist");

            Assert.False(success);
        }

        [Fact]
        public void RevokeAllSessionsForUser_RevokesOnlyThatUsersActiveSessions()
        {
            var context = CreateContext();
            var repository = new AuthRepository(context, CreateMapper());
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            repository.CreateSession(userId, "u", "TeamMember", "t1", DateTime.UtcNow.AddDays(7));
            repository.CreateSession(userId, "u", "TeamMember", "t2", DateTime.UtcNow.AddDays(7));
            repository.CreateSession(otherUserId, "other", "TeamMember", "t3", DateTime.UtcNow.AddDays(7));

            var revokedCount = repository.RevokeAllSessionsForUser(userId);

            Assert.Equal(2, revokedCount);
            Assert.All(context.AuthSessions.Where(s => s.UserId == userId), s => Assert.True(s.IsRevoked));
            Assert.False(context.AuthSessions.First(s => s.UserId == otherUserId).IsRevoked);
        }
    }
}
