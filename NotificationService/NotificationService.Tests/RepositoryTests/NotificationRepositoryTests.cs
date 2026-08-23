using NotificationService.Data;
using NotificationService.Exceptions;
using NotificationService.Models;
using NotificationService.Tests.TestHelpers;

namespace NotificationService.Tests.RepositoryTests
{
    public class NotificationRepositoryTests
    {
        [Fact]
        public async Task CreateAsync_SetsIdCreatedAtAndUnread()
        {
            var repository = new NotificationRepository(DbContextFactory.CreateContext());
            var userId = Guid.NewGuid();

            var created = await repository.CreateAsync(new Notification
            {
                UserId = userId,
                Description = "Poruka",
                Type = "Test"
            });

            Assert.NotEqual(Guid.Empty, created.Id);
            Assert.False(created.IsRead);
            Assert.True(created.CreatedAt > DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsOnlyNotificationsForThatUser_NewestFirst()
        {
            var repository = new NotificationRepository(DbContextFactory.CreateContext());
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            var first = await repository.CreateAsync(new Notification { UserId = userId, Description = "Prva", Type = "Test" });
            await Task.Delay(10);
            var second = await repository.CreateAsync(new Notification { UserId = userId, Description = "Druga", Type = "Test" });
            await repository.CreateAsync(new Notification { UserId = otherUserId, Description = "Tudja", Type = "Test" });

            var result = (await repository.GetByUserIdAsync(userId)).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal(second.Id, result[0].Id);
            Assert.Equal(first.Id, result[1].Id);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
        {
            var repository = new NotificationRepository(DbContextFactory.CreateContext());

            var result = await repository.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task MarkAsReadAsync_SetsIsReadTrue()
        {
            var repository = new NotificationRepository(DbContextFactory.CreateContext());
            var created = await repository.CreateAsync(new Notification
            {
                UserId = Guid.NewGuid(),
                Description = "Poruka",
                Type = "Test"
            });

            var updated = await repository.MarkAsReadAsync(created.Id);

            Assert.True(updated.IsRead);
        }

        [Fact]
        public async Task MarkAsReadAsync_WhenNotFound_ThrowsEntityNotFoundException()
        {
            var repository = new NotificationRepository(DbContextFactory.CreateContext());

            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => repository.MarkAsReadAsync(Guid.NewGuid()));
        }
    }
}
