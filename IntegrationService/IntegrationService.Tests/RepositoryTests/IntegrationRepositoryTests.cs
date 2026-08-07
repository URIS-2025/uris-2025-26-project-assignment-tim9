using IntegrationService.Data;
using IntegrationService.Exceptions;
using IntegrationService.Models;
using IntegrationService.Tests.TestHelpers;

namespace IntegrationService.Tests.RepositoryTests
{
    public class IntegrationRepositoryTests
    {
        [Fact]
        public async Task CreateAsync_SetsIdCreatedAtAndActiveStatus()
        {
            var repository = new IntegrationRepository(DbContextFactory.CreateContext());

            var created = await repository.CreateAsync(new Integration
            {
                Type = "GitHub",
                ApiKeyEncrypted = "encrypted-value"
            });

            Assert.NotEqual(Guid.Empty, created.Id);
            Assert.True(created.Status);
            Assert.True(created.CreatedAt > DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
        {
            var repository = new IntegrationRepository(DbContextFactory.CreateContext());

            var result = await repository.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_WithRotateApiKeyFalse_KeepsExistingEncryptedKey()
        {
            var repository = new IntegrationRepository(DbContextFactory.CreateContext());
            var created = await repository.CreateAsync(new Integration { Type = "GitHub", ApiKeyEncrypted = "original-encrypted" });

            var updated = await repository.UpdateAsync(
                created.Id,
                new Integration { Type = "GitHub-Renamed", ApiKeyEncrypted = "ignored-because-rotate-is-false", Status = false },
                rotateApiKey: false);

            Assert.Equal("GitHub-Renamed", updated.Type);
            Assert.Equal("original-encrypted", updated.ApiKeyEncrypted);
            Assert.False(updated.Status);
        }

        [Fact]
        public async Task UpdateAsync_WithRotateApiKeyTrue_ReplacesEncryptedKey()
        {
            var repository = new IntegrationRepository(DbContextFactory.CreateContext());
            var created = await repository.CreateAsync(new Integration { Type = "GitHub", ApiKeyEncrypted = "original-encrypted" });

            var updated = await repository.UpdateAsync(
                created.Id,
                new Integration { Type = "GitHub", ApiKeyEncrypted = "new-encrypted", Status = true },
                rotateApiKey: true);

            Assert.Equal("new-encrypted", updated.ApiKeyEncrypted);
        }

        [Fact]
        public async Task UpdateAsync_WhenNotFound_ThrowsEntityNotFoundException()
        {
            var repository = new IntegrationRepository(DbContextFactory.CreateContext());

            await Assert.ThrowsAsync<EntityNotFoundException>(() => repository.UpdateAsync(
                Guid.NewGuid(), new Integration { Type = "X", ApiKeyEncrypted = "y" }, rotateApiKey: false));
        }

        [Fact]
        public async Task DeleteAsync_RemovesIntegration()
        {
            var repository = new IntegrationRepository(DbContextFactory.CreateContext());
            var created = await repository.CreateAsync(new Integration { Type = "GitHub", ApiKeyEncrypted = "x" });

            await repository.DeleteAsync(created.Id);

            Assert.Null(await repository.GetByIdAsync(created.Id));
        }

        [Fact]
        public async Task DeleteAsync_WhenNotFound_ThrowsEntityNotFoundException()
        {
            var repository = new IntegrationRepository(DbContextFactory.CreateContext());

            await Assert.ThrowsAsync<EntityNotFoundException>(() => repository.DeleteAsync(Guid.NewGuid()));
        }
    }
}
