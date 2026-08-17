using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TimelogService.Context;
using TimelogService.Data;
using TimelogService.Models;
using TimelogService.Models.DTO;
using TimelogService.Models.DTO.Project;
using TimelogService.Models.DTO.WorkPackage;
using TimelogService.Profiles;
using TimelogService.ServiceCalls.Project;
using TimelogService.ServiceCalls.WorkPackage;

namespace TimelogService.Tests
{
    public class TimelogRepositoryTests
    {
        private static TimelogContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<TimelogContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var configuration = new ConfigurationBuilder().Build();

            return new TimelogContext(options, configuration);
        }

        private static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<TimelogProfile>(), NullLoggerFactory.Instance);
            return config.CreateMapper();
        }

        private sealed class Fixture
        {
            public TimelogContext Context { get; }
            public TimelogRepository Repository { get; }
            public Mock<IProjectService> ProjectService { get; }
            public Mock<IWorkPackageService> WorkPackageService { get; }

            public Fixture()
            {
                Context = CreateContext();
                ProjectService = new Mock<IProjectService>();
                WorkPackageService = new Mock<IWorkPackageService>();
                Repository = new TimelogRepository(Context, CreateMapper(), ProjectService.Object, WorkPackageService.Object);
            }
        }

        // ---------- GetTimelogs ----------

        [Fact]
        public void GetTimelogs_WithNoFilter_ReturnsAll()
        {
            var fx = new Fixture();
            fx.Context.Timelogs.AddRange(
                new Timelog { Id = Guid.NewGuid(), ProjectId = Guid.NewGuid(), WorkPackageId = Guid.NewGuid() },
                new Timelog { Id = Guid.NewGuid(), ProjectId = Guid.NewGuid(), WorkPackageId = Guid.NewGuid() }
            );
            fx.Context.SaveChanges();

            var result = fx.Repository.GetTimelogs();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void GetTimelogs_FilteredByProjectId_ReturnsOnlyThatProject()
        {
            var fx = new Fixture();
            var projectId = Guid.NewGuid();
            fx.Context.Timelogs.AddRange(
                new Timelog { Id = Guid.NewGuid(), ProjectId = projectId, WorkPackageId = Guid.NewGuid(), HoursSpent = 1 },
                new Timelog { Id = Guid.NewGuid(), ProjectId = Guid.NewGuid(), WorkPackageId = Guid.NewGuid(), HoursSpent = 2 }
            );
            fx.Context.SaveChanges();

            var result = fx.Repository.GetTimelogs(projectId: projectId).ToList();

            Assert.Single(result);
            Assert.Equal(1, result[0].HoursSpent);
        }

        [Fact]
        public void GetTimelogs_FilteredByWorkPackageId_ReturnsOnlyThatWorkPackage()
        {
            var fx = new Fixture();
            var workPackageId = Guid.NewGuid();
            fx.Context.Timelogs.AddRange(
                new Timelog { Id = Guid.NewGuid(), ProjectId = Guid.NewGuid(), WorkPackageId = workPackageId, HoursSpent = 1 },
                new Timelog { Id = Guid.NewGuid(), ProjectId = Guid.NewGuid(), WorkPackageId = Guid.NewGuid(), HoursSpent = 2 }
            );
            fx.Context.SaveChanges();

            var result = fx.Repository.GetTimelogs(workPackageId: workPackageId).ToList();

            Assert.Single(result);
            Assert.Equal(1, result[0].HoursSpent);
        }

        // ---------- GetTimelogById ----------

        [Fact]
        public void GetTimelogById_ForExisting_ReturnsIt()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            fx.Context.Timelogs.Add(new Timelog { Id = id, ProjectId = Guid.NewGuid(), WorkPackageId = Guid.NewGuid() });
            fx.Context.SaveChanges();

            var result = fx.Repository.GetTimelogById(id);

            Assert.NotNull(result);
            Assert.Equal(id, result!.Id);
        }

        [Fact]
        public void GetTimelogById_ForNonexistent_ReturnsNull()
        {
            var fx = new Fixture();

            var result = fx.Repository.GetTimelogById(Guid.NewGuid());

            Assert.Null(result);
        }

        // ---------- CreateTimelogAsync ----------

        [Fact]
        public async Task CreateTimelogAsync_PersistsWithLoggedByUserIdFromParameter()
        {
            var fx = new Fixture();
            var userId = Guid.NewGuid();
            fx.ProjectService.Setup(s => s.GetUserInfoAsync(userId)).ReturnsAsync((UserInfoDTO?)null);
            fx.WorkPackageService.Setup(s => s.GetWorkPackageByIdAsync(It.IsAny<Guid>())).ReturnsAsync((WorkPackageDTO?)null);
            var dto = new TimelogCreationDTO
            {
                ProjectId = Guid.NewGuid(),
                WorkPackageId = Guid.NewGuid(),
                HoursSpent = 4,
                Date = new DateTime(2026, 1, 1)
            };

            var result = await fx.Repository.CreateTimelogAsync(dto, userId);

            Assert.NotEqual(Guid.Empty, result.Id);
            var persisted = fx.Context.Timelogs.Single(t => t.Id == result.Id);
            Assert.Equal(userId, persisted.LoggedByUserId);
        }

        [Fact]
        public async Task CreateTimelogAsync_WhenDependenciesHaveData_EnrichesConfirmation()
        {
            var fx = new Fixture();
            var userId = Guid.NewGuid();
            fx.ProjectService.Setup(s => s.GetUserInfoAsync(userId))
                .ReturnsAsync(new UserInfoDTO { Username = "mila", Email = "mila@example.com" });
            fx.WorkPackageService.Setup(s => s.GetWorkPackageByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new WorkPackageDTO { Title = "Fix the bug", Status = "InProgress" });
            var dto = new TimelogCreationDTO
            {
                ProjectId = Guid.NewGuid(),
                WorkPackageId = Guid.NewGuid(),
                HoursSpent = 4,
                Date = new DateTime(2026, 1, 1)
            };

            var result = await fx.Repository.CreateTimelogAsync(dto, userId);

            Assert.Equal("mila", result.Username);
            Assert.Equal("Fix the bug", result.WorkPackageTitle);
        }

        [Fact]
        public async Task CreateTimelogAsync_WhenDependenciesReturnNull_StillSucceedsWithPlaceholderText()
        {
            // Reproduces the real bug found during manual testing: an unreachable Project/
            // WorkPackage Service must not prevent timelog creation from succeeding.
            var fx = new Fixture();
            var userId = Guid.NewGuid();
            fx.ProjectService.Setup(s => s.GetUserInfoAsync(It.IsAny<Guid>())).ReturnsAsync((UserInfoDTO?)null);
            fx.WorkPackageService.Setup(s => s.GetWorkPackageByIdAsync(It.IsAny<Guid>())).ReturnsAsync((WorkPackageDTO?)null);
            var dto = new TimelogCreationDTO
            {
                ProjectId = Guid.NewGuid(),
                WorkPackageId = Guid.NewGuid(),
                HoursSpent = 4,
                Date = new DateTime(2026, 1, 1)
            };

            var result = await fx.Repository.CreateTimelogAsync(dto, userId);

            Assert.Equal("Unknown User", result.Username);
            Assert.Equal("Unknown WorkPackage", result.WorkPackageTitle);
            Assert.Single(fx.Context.Timelogs);
        }

        // ---------- UpdateTimelogAsync ----------

        [Fact]
        public async Task UpdateTimelogAsync_WithPartialData_OnlyChangesSuppliedFields()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var originalProjectId = Guid.NewGuid();
            var originalWorkPackageId = Guid.NewGuid();
            fx.Context.Timelogs.Add(new Timelog
            {
                Id = id,
                ProjectId = originalProjectId,
                WorkPackageId = originalWorkPackageId,
                HoursSpent = 3,
                Date = new DateTime(2026, 1, 1),
                LoggedByUserId = Guid.NewGuid()
            });
            fx.Context.SaveChanges();
            fx.ProjectService.Setup(s => s.GetUserInfoAsync(It.IsAny<Guid>())).ReturnsAsync((UserInfoDTO?)null);
            fx.WorkPackageService.Setup(s => s.GetWorkPackageByIdAsync(It.IsAny<Guid>())).ReturnsAsync((WorkPackageDTO?)null);

            var result = await fx.Repository.UpdateTimelogAsync(id, new TimelogUpdateDTO { HoursSpent = 7.5 });

            Assert.NotNull(result);
            Assert.Equal(7.5, result!.HoursSpent);

            var persisted = fx.Context.Timelogs.Single(t => t.Id == id);
            Assert.Equal(7.5, persisted.HoursSpent);
            Assert.Equal(originalProjectId, persisted.ProjectId);
            Assert.Equal(originalWorkPackageId, persisted.WorkPackageId);
        }

        [Fact]
        public async Task UpdateTimelogAsync_ForNonexistentId_ReturnsNullWithoutThrowing()
        {
            var fx = new Fixture();

            var result = await fx.Repository.UpdateTimelogAsync(Guid.NewGuid(), new TimelogUpdateDTO { HoursSpent = 5 });

            Assert.Null(result);
            fx.ProjectService.Verify(s => s.GetUserInfoAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task UpdateTimelogAsync_AlsoFetchesWorkPackageTitle()
        {
            // Reproduces a pre-existing inconsistency found while fixing this: the original
            // code only ever fetched Username on update, never WorkPackageTitle.
            var fx = new Fixture();
            var id = Guid.NewGuid();
            fx.Context.Timelogs.Add(new Timelog { Id = id, ProjectId = Guid.NewGuid(), WorkPackageId = Guid.NewGuid() });
            fx.Context.SaveChanges();
            fx.ProjectService.Setup(s => s.GetUserInfoAsync(It.IsAny<Guid>())).ReturnsAsync((UserInfoDTO?)null);
            fx.WorkPackageService.Setup(s => s.GetWorkPackageByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new WorkPackageDTO { Title = "Updated Task Title" });

            var result = await fx.Repository.UpdateTimelogAsync(id, new TimelogUpdateDTO { HoursSpent = 2 });

            Assert.Equal("Updated Task Title", result!.WorkPackageTitle);
        }

        // ---------- DeleteTimelog ----------

        [Fact]
        public void DeleteTimelog_RemovesTheRow()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            fx.Context.Timelogs.Add(new Timelog { Id = id, ProjectId = Guid.NewGuid(), WorkPackageId = Guid.NewGuid() });
            fx.Context.SaveChanges();

            fx.Repository.DeleteTimelog(id);

            Assert.Empty(fx.Context.Timelogs);
        }

        [Fact]
        public void DeleteTimelog_ForNonexistentId_DoesNotThrow()
        {
            var fx = new Fixture();

            var exception = Record.Exception(() => fx.Repository.DeleteTimelog(Guid.NewGuid()));

            Assert.Null(exception);
        }
    }
}
