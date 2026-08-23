using WorkPackageService.Context;
using WorkPackageService.Data;
using WorkPackageService.Models.DTO.DependencyDTOs;
using WorkPackageService.Tests.TestHelpers;

namespace WorkPackageService.Tests.RepositoryTests
{
    public class DependencyRepositoryTests
    {
        private static DependencyRepository CreateRepository(WorkPackageServiceContext context)
        {
            var mapper = DbContextFactory.CreateMapper();
            return new DependencyRepository(context, mapper);
        }

        [Fact]
        public void Add_WhenTaskIdEqualsBlockerTaskId_ReturnsNull()
        {
            // Arrange
            using var context = DbContextFactory.CreateContext();
            var repository = CreateRepository(context);
            var taskId = Guid.NewGuid();
            var dto = new DependencyCreateDTO { TaskId = taskId, BlockerTaskId = taskId };

            // Act
            var result = repository.Add(dto);

            // Assert
            Assert.Null(result);
            Assert.Empty(context.Dependencies);
        }

        [Fact]
        public void Add_WhenTaskIdDiffersFromBlockerTaskId_CreatesDependency()
        {
            // Arrange
            using var context = DbContextFactory.CreateContext();
            var repository = CreateRepository(context);
            var dto = new DependencyCreateDTO { TaskId = Guid.NewGuid(), BlockerTaskId = Guid.NewGuid() };

            // Act
            var result = repository.Add(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Single(context.Dependencies);
        }
    }
}
