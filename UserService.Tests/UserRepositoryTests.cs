using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UserService.Context;
using UserService.Data;
using UserService.Models;
using UserService.Models.DTO.UserDtos;
using UserService.Models.Enums;
using UserService.Profiles;
using UserService.ServiceCalls.Notification;
using UserService.Services;
using Xunit;

namespace UserService.Tests
{
    public class UserRepositoryTests
    {
        private static UserContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<UserContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var configuration = new ConfigurationBuilder().Build();
            return new UserContext(options, configuration);
        }

        private static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<UserProfile>(), NullLoggerFactory.Instance);
            return config.CreateMapper();
        }

        private static UserRepository CreateRepository(UserContext context, Mock<INotificationService>? notificationMock = null)
        {
            notificationMock ??= new Mock<INotificationService>();
            notificationMock
                .Setup(n => n.SendNotificationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            return new UserRepository(context, CreateMapper(), new PasswordService(), notificationMock.Object);
        }

        [Fact]
        public void CreateUser_HashesPassword_DoesNotStorePlainText()
        {
            var context = CreateContext();
            var repository = CreateRepository(context);

            var dto = new UserCreationDto
            {
                Name = "Test User",
                Username = "testuser",
                Email = "test@example.com",
                ContactInfo = "123",
                Password = "PlainPassword123"
            };

            var result = repository.CreateUser(dto);

            var stored = context.Users.First(u => u.UserId == result.UserId);
            Assert.NotEqual("PlainPassword123", stored.PasswordHash);
            Assert.False(string.IsNullOrEmpty(stored.Salt));
            Assert.Equal(UserRole.TeamMember, stored.Role);
        }

        [Fact]
        public void ValidateCredentials_CorrectPassword_ReturnsValidTrue()
        {
            var context = CreateContext();
            var repository = CreateRepository(context);
            repository.CreateUser(new UserCreationDto
            {
                Name = "Test User",
                Username = "testuser",
                Email = "test@example.com",
                ContactInfo = "123",
                Password = "CorrectPassword1"
            });

            var result = repository.ValidateCredentials("testuser", "CorrectPassword1");

            Assert.True(result.IsValid);
            Assert.Equal("testuser", result.Username);
        }

        [Fact]
        public void ValidateCredentials_WrongPassword_ReturnsValidFalse()
        {
            var context = CreateContext();
            var repository = CreateRepository(context);
            repository.CreateUser(new UserCreationDto
            {
                Name = "Test User",
                Username = "testuser",
                Email = "test@example.com",
                ContactInfo = "123",
                Password = "CorrectPassword1"
            });

            var result = repository.ValidateCredentials("testuser", "WrongPassword");

            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task ChangeRole_AdminCannotRevokeOwnAdminRights()
        {
            var context = CreateContext();
            var adminId = Guid.NewGuid();
            context.Users.Add(new User
            {
                UserId = adminId,
                Name = "Admin",
                Username = "admin",
                Email = "admin@example.com",
                ContactInfo = "123",
                PasswordHash = "hash",
                Salt = "salt",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var repository = CreateRepository(context);

            var success = await repository.ChangeRole(new RoleUpdateDto
            {
                UserId = adminId,
                NewRole = UserRole.TeamMember,
                ChangedBy = adminId
            });

            Assert.False(success);
            Assert.Equal(UserRole.Admin, context.Users.First(u => u.UserId == adminId).Role);
        }

        [Fact]
        public async Task ChangeRole_ValidChange_UpdatesRoleAndSendsNotification()
        {
            var context = CreateContext();
            var adminId = Guid.NewGuid();
            var targetId = Guid.NewGuid();
            context.Users.AddRange(
                new User { UserId = adminId, Name = "Admin", Username = "admin", Email = "a@a.com", ContactInfo = "1", PasswordHash = "h", Salt = "s", Role = UserRole.Admin, IsActive = true, CreatedAt = DateTime.UtcNow },
                new User { UserId = targetId, Name = "Member", Username = "member", Email = "m@m.com", ContactInfo = "2", PasswordHash = "h", Salt = "s", Role = UserRole.TeamMember, IsActive = true, CreatedAt = DateTime.UtcNow }
            );
            context.SaveChanges();

            var notificationMock = new Mock<INotificationService>();
            notificationMock
                .Setup(n => n.SendNotificationAsync(targetId, It.IsAny<string>(), "RoleChanged"))
                .ReturnsAsync(true);

            var repository = CreateRepository(context, notificationMock);

            var success = await repository.ChangeRole(new RoleUpdateDto
            {
                UserId = targetId,
                NewRole = UserRole.ProjectManager,
                ChangedBy = adminId
            });

            Assert.True(success);
            Assert.Equal(UserRole.ProjectManager, context.Users.First(u => u.UserId == targetId).Role);
            notificationMock.Verify(n => n.SendNotificationAsync(targetId, It.IsAny<string>(), "RoleChanged"), Times.Once);
        }

        [Fact]
        public void SetActiveStatus_Deactivate_LogsActivity()
        {
            var context = CreateContext();
            var userId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            context.Users.Add(new User { UserId = userId, Name = "U", Username = "u", Email = "u@u.com", ContactInfo = "1", PasswordHash = "h", Salt = "s", Role = UserRole.TeamMember, IsActive = true, CreatedAt = DateTime.UtcNow });
            context.SaveChanges();

            var repository = CreateRepository(context);
            var success = repository.SetActiveStatus(userId, false, adminId);

            Assert.True(success);
            Assert.False(context.Users.First(u => u.UserId == userId).IsActive);
            var log = context.UserActivityLogs.First(l => l.UserId == userId);
            Assert.Equal("Deactivated", log.Action);
            Assert.Equal(adminId, log.PerformedBy);
        }

        [Fact]
        public void GetUsers_FilterByRole_ReturnsOnlyMatching()
        {
            var context = CreateContext();
            context.Users.AddRange(
                new User { UserId = Guid.NewGuid(), Name = "A", Username = "a", Email = "a@a.com", ContactInfo = "1", PasswordHash = "h", Salt = "s", Role = UserRole.Admin, IsActive = true, CreatedAt = DateTime.UtcNow },
                new User { UserId = Guid.NewGuid(), Name = "B", Username = "b", Email = "b@b.com", ContactInfo = "2", PasswordHash = "h", Salt = "s", Role = UserRole.Client, IsActive = true, CreatedAt = DateTime.UtcNow }
            );
            context.SaveChanges();

            var repository = CreateRepository(context);
            var result = repository.GetUsers(null, UserRole.Client, null);

            Assert.Single(result);
            Assert.Equal("b", result.First().Username);
        }
    }
}
