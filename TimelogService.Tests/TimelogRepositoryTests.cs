using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TimelogService.Context;
using TimelogService.Data;
using TimelogService.Exceptions;
using TimelogService.Models;
using TimelogService.Models.DTO;
using TimelogService.Models.DTO.User;
using TimelogService.Models.DTO.WorkPackage;
using TimelogService.Profiles;
using TimelogService.ServiceCalls.Project;
using TimelogService.ServiceCalls.User;
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
            public Mock<IUserService> UserService { get; }
            public Mock<ITaskService> TaskService { get; }
            public Mock<IProjectService> ProjectService { get; }

            public Fixture()
            {
                Context = CreateContext();
                UserService = new Mock<IUserService>();
                TaskService = new Mock<ITaskService>();
                ProjectService = new Mock<IProjectService>();
                TaskService.Setup(s => s.GetTaskByIdAsync(It.IsAny<Guid>(), It.IsAny<string?>()))
                    .ReturnsAsync(new TaskLookupResult(TaskLookupStatus.Found, new TaskDTO { Title = "Some Task" }));
                ProjectService.Setup(s => s.CheckMembershipAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string?>()))
                    .ReturnsAsync(new ProjectMembershipResult(ProjectMembershipStatus.Member));
                ProjectService.Setup(s => s.CheckProjectExistsAsync(It.IsAny<Guid>(), It.IsAny<string?>()))
                    .ReturnsAsync(new ProjectExistsResult(ProjectExistsStatus.Exists));
                Repository = new TimelogRepository(Context, CreateMapper(), UserService.Object, TaskService.Object, ProjectService.Object);
            }
        }

        // ---------- GetTimelogs ----------

        [Fact]
        public void GetTimelogs_WithNoFilter_ReturnsAll()
        {
            var fx = new Fixture();
            fx.Context.Timelogs.AddRange(
                new Timelog { Id = Guid.NewGuid(), ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid() },
                new Timelog { Id = Guid.NewGuid(), ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid() }
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
                new Timelog { Id = Guid.NewGuid(), ProjectId = projectId, TaskId = Guid.NewGuid(), HoursSpent = 1 },
                new Timelog { Id = Guid.NewGuid(), ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid(), HoursSpent = 2 }
            );
            fx.Context.SaveChanges();

            var result = fx.Repository.GetTimelogs(projectId: projectId).ToList();

            Assert.Single(result);
            Assert.Equal(1, result[0].HoursSpent);
        }

        [Fact]
        public void GetTimelogs_FilteredByTaskId_ReturnsOnlyThatTask()
        {
            var fx = new Fixture();
            var taskId = Guid.NewGuid();
            fx.Context.Timelogs.AddRange(
                new Timelog { Id = Guid.NewGuid(), ProjectId = Guid.NewGuid(), TaskId = taskId, HoursSpent = 1 },
                new Timelog { Id = Guid.NewGuid(), ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid(), HoursSpent = 2 }
            );
            fx.Context.SaveChanges();

            var result = fx.Repository.GetTimelogs(taskId: taskId).ToList();

            Assert.Single(result);
            Assert.Equal(1, result[0].HoursSpent);
        }

        // ---------- GetTimelogById ----------

        [Fact]
        public void GetTimelogById_ForExisting_ReturnsIt()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            fx.Context.Timelogs.Add(new Timelog { Id = id, ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid() });
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
            fx.UserService.Setup(s => s.GetUserInfoAsync(userId)).ReturnsAsync((UserInfoDTO?)null);
            var dto = new TimelogCreationDTO
            {
                ProjectId = Guid.NewGuid(),
                TaskId = Guid.NewGuid(),
                HoursSpent = 4,
                Date = new DateTime(2026, 1, 1)
            };

            var result = await fx.Repository.CreateTimelogAsync(dto, userId, bearerToken: null);

            Assert.NotEqual(Guid.Empty, result.Id);
            var persisted = fx.Context.Timelogs.Single(t => t.Id == result.Id);
            Assert.Equal(userId, persisted.LoggedByUserId);
        }

        [Fact]
        public async Task CreateTimelogAsync_WhenDependenciesHaveData_EnrichesConfirmation()
        {
            var fx = new Fixture();
            var userId = Guid.NewGuid();
            fx.UserService.Setup(s => s.GetUserInfoAsync(userId))
                .ReturnsAsync(new UserInfoDTO { Username = "mila", Email = "mila@example.com", Role = "TeamMember" });
            fx.TaskService.Setup(s => s.GetTaskByIdAsync(It.IsAny<Guid>(), It.IsAny<string?>()))
                .ReturnsAsync(new TaskLookupResult(TaskLookupStatus.Found, new TaskDTO { Title = "Fix the bug", Status = "InProgress" }));
            var dto = new TimelogCreationDTO
            {
                ProjectId = Guid.NewGuid(),
                TaskId = Guid.NewGuid(),
                HoursSpent = 4,
                Date = new DateTime(2026, 1, 1)
            };

            var result = await fx.Repository.CreateTimelogAsync(dto, userId, bearerToken: null);

            Assert.Equal("mila", result.Username);
            Assert.Equal("mila@example.com", result.Email);
            Assert.Equal("TeamMember", result.UserRole);
            Assert.Equal("Fix the bug", result.TaskTitle);
            Assert.Equal("InProgress", result.TaskStatus);
        }

        [Fact]
        public async Task CreateTimelogAsync_WhenTaskServiceUnreachable_StillSucceedsWithPlaceholderText()
        {
            // Reproduces the real bug found during manual testing: an unreachable User/Task
            // lookup must not prevent timelog creation from succeeding.
            var fx = new Fixture();
            var userId = Guid.NewGuid();
            fx.UserService.Setup(s => s.GetUserInfoAsync(It.IsAny<Guid>())).ReturnsAsync((UserInfoDTO?)null);
            fx.TaskService.Setup(s => s.GetTaskByIdAsync(It.IsAny<Guid>(), It.IsAny<string?>()))
                .ReturnsAsync(new TaskLookupResult(TaskLookupStatus.ServiceUnavailable, null));
            var dto = new TimelogCreationDTO
            {
                ProjectId = Guid.NewGuid(),
                TaskId = Guid.NewGuid(),
                HoursSpent = 4,
                Date = new DateTime(2026, 1, 1)
            };

            var result = await fx.Repository.CreateTimelogAsync(dto, userId, bearerToken: null);

            Assert.Equal("Unknown User", result.Username);
            Assert.Equal("Unknown", result.Email);
            Assert.Equal("Unknown", result.UserRole);
            Assert.Equal("Unknown Task", result.TaskTitle);
            Assert.Equal("Unknown", result.TaskStatus);
            Assert.Single(fx.Context.Timelogs);
        }

        [Fact]
        public async Task CreateTimelogAsync_WhenTaskConfirmedNotFound_ThrowsAndDoesNotPersist()
        {
            var fx = new Fixture();
            var taskId = Guid.NewGuid();
            fx.TaskService.Setup(s => s.GetTaskByIdAsync(taskId, It.IsAny<string?>()))
                .ReturnsAsync(new TaskLookupResult(TaskLookupStatus.NotFound, null));
            var dto = new TimelogCreationDTO
            {
                ProjectId = Guid.NewGuid(),
                TaskId = taskId,
                HoursSpent = 4,
                Date = new DateTime(2026, 1, 1)
            };

            await Assert.ThrowsAsync<TaskNotFoundException>(
                () => fx.Repository.CreateTimelogAsync(dto, Guid.NewGuid(), bearerToken: null));

            Assert.Empty(fx.Context.Timelogs);
        }

        [Fact]
        public async Task CreateTimelogAsync_ForwardsBearerTokenToTaskService()
        {
            var fx = new Fixture();
            var dto = new TimelogCreationDTO { ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid(), HoursSpent = 1, Date = new DateTime(2026, 1, 1) };

            await fx.Repository.CreateTimelogAsync(dto, Guid.NewGuid(), bearerToken: "abc123");

            fx.TaskService.Verify(s => s.GetTaskByIdAsync(dto.TaskId, "abc123"), Times.Once);
        }

        [Fact]
        public async Task CreateTimelogAsync_WhenUserNotAProjectMember_ThrowsAndDoesNotPersist()
        {
            var fx = new Fixture();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            fx.ProjectService.Setup(s => s.CheckMembershipAsync(projectId, userId, It.IsAny<string?>()))
                .ReturnsAsync(new ProjectMembershipResult(ProjectMembershipStatus.NotMember));
            var dto = new TimelogCreationDTO
            {
                ProjectId = projectId,
                TaskId = Guid.NewGuid(),
                HoursSpent = 4,
                Date = new DateTime(2026, 1, 1)
            };

            await Assert.ThrowsAsync<UserNotProjectMemberException>(
                () => fx.Repository.CreateTimelogAsync(dto, userId, bearerToken: null));

            Assert.Empty(fx.Context.Timelogs);
            fx.TaskService.Verify(s => s.GetTaskByIdAsync(It.IsAny<Guid>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task CreateTimelogAsync_WhenProjectServiceUnreachable_StillSucceeds()
        {
            var fx = new Fixture();
            fx.ProjectService.Setup(s => s.CheckMembershipAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string?>()))
                .ReturnsAsync(new ProjectMembershipResult(ProjectMembershipStatus.ServiceUnavailable));
            var dto = new TimelogCreationDTO { ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid(), HoursSpent = 1, Date = new DateTime(2026, 1, 1) };

            var result = await fx.Repository.CreateTimelogAsync(dto, Guid.NewGuid(), bearerToken: null);

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Single(fx.Context.Timelogs);
        }

        [Fact]
        public async Task CreateTimelogAsync_ForwardsBearerTokenToProjectService()
        {
            var fx = new Fixture();
            var dto = new TimelogCreationDTO { ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid(), HoursSpent = 1, Date = new DateTime(2026, 1, 1) };
            var userId = Guid.NewGuid();

            await fx.Repository.CreateTimelogAsync(dto, userId, bearerToken: "abc123");

            fx.ProjectService.Verify(s => s.CheckMembershipAsync(dto.ProjectId, userId, "abc123"), Times.Once);
        }

        [Fact]
        public async Task CreateTimelogAsync_WhenUserHasClientRole_ThrowsAndDoesNotPersist()
        {
            var fx = new Fixture();
            var userId = Guid.NewGuid();
            fx.UserService.Setup(s => s.GetUserInfoAsync(userId))
                .ReturnsAsync(new UserInfoDTO { Username = "client.user", Role = "Client" });
            var dto = new TimelogCreationDTO { ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid(), HoursSpent = 1, Date = new DateTime(2026, 1, 1) };

            await Assert.ThrowsAsync<ClientCannotLogTimeException>(
                () => fx.Repository.CreateTimelogAsync(dto, userId, bearerToken: null));

            Assert.Empty(fx.Context.Timelogs);
            fx.ProjectService.Verify(s => s.CheckMembershipAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task CreateTimelogAsync_WhenUserIsAdmin_BypassesMembershipCheck()
        {
            var fx = new Fixture();
            var userId = Guid.NewGuid();
            fx.UserService.Setup(s => s.GetUserInfoAsync(userId))
                .ReturnsAsync(new UserInfoDTO { Username = "admin.user", Role = "Admin" });
            fx.ProjectService.Setup(s => s.CheckMembershipAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string?>()))
                .ReturnsAsync(new ProjectMembershipResult(ProjectMembershipStatus.NotMember));
            var dto = new TimelogCreationDTO { ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid(), HoursSpent = 1, Date = new DateTime(2026, 1, 1) };

            var result = await fx.Repository.CreateTimelogAsync(dto, userId, bearerToken: null);

            Assert.NotEqual(Guid.Empty, result.Id);
            fx.ProjectService.Verify(s => s.CheckMembershipAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task CreateTimelogAsync_WhenProjectConfirmedNotFound_ThrowsAndDoesNotPersist()
        {
            var fx = new Fixture();
            var projectId = Guid.NewGuid();
            fx.ProjectService.Setup(s => s.CheckProjectExistsAsync(projectId, It.IsAny<string?>()))
                .ReturnsAsync(new ProjectExistsResult(ProjectExistsStatus.NotFound));
            var dto = new TimelogCreationDTO { ProjectId = projectId, TaskId = Guid.NewGuid(), HoursSpent = 1, Date = new DateTime(2026, 1, 1) };

            await Assert.ThrowsAsync<ProjectNotFoundException>(
                () => fx.Repository.CreateTimelogAsync(dto, Guid.NewGuid(), bearerToken: null));

            Assert.Empty(fx.Context.Timelogs);
        }

        [Fact]
        public async Task CreateTimelogAsync_WhenProjectConfirmedNotFound_BlocksEvenAdmins()
        {
            var fx = new Fixture();
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            fx.UserService.Setup(s => s.GetUserInfoAsync(userId))
                .ReturnsAsync(new UserInfoDTO { Username = "admin.user", Role = "Admin" });
            fx.ProjectService.Setup(s => s.CheckProjectExistsAsync(projectId, It.IsAny<string?>()))
                .ReturnsAsync(new ProjectExistsResult(ProjectExistsStatus.NotFound));
            var dto = new TimelogCreationDTO { ProjectId = projectId, TaskId = Guid.NewGuid(), HoursSpent = 1, Date = new DateTime(2026, 1, 1) };

            await Assert.ThrowsAsync<ProjectNotFoundException>(
                () => fx.Repository.CreateTimelogAsync(dto, userId, bearerToken: null));

            Assert.Empty(fx.Context.Timelogs);
        }

        [Fact]
        public async Task CreateTimelogAsync_WhenProjectServiceUnreachableForExistenceCheck_StillSucceeds()
        {
            var fx = new Fixture();
            fx.ProjectService.Setup(s => s.CheckProjectExistsAsync(It.IsAny<Guid>(), It.IsAny<string?>()))
                .ReturnsAsync(new ProjectExistsResult(ProjectExistsStatus.ServiceUnavailable));
            var dto = new TimelogCreationDTO { ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid(), HoursSpent = 1, Date = new DateTime(2026, 1, 1) };

            var result = await fx.Repository.CreateTimelogAsync(dto, Guid.NewGuid(), bearerToken: null);

            Assert.NotEqual(Guid.Empty, result.Id);
        }

        [Fact]
        public async Task CreateTimelogAsync_ChecksProjectExistenceBeforeMembershipOrTask()
        {
            var fx = new Fixture();
            var projectId = Guid.NewGuid();
            fx.ProjectService.Setup(s => s.CheckProjectExistsAsync(projectId, It.IsAny<string?>()))
                .ReturnsAsync(new ProjectExistsResult(ProjectExistsStatus.NotFound));
            var dto = new TimelogCreationDTO { ProjectId = projectId, TaskId = Guid.NewGuid(), HoursSpent = 1, Date = new DateTime(2026, 1, 1) };

            await Assert.ThrowsAsync<ProjectNotFoundException>(
                () => fx.Repository.CreateTimelogAsync(dto, Guid.NewGuid(), bearerToken: null));

            fx.UserService.Verify(s => s.GetUserInfoAsync(It.IsAny<Guid>()), Times.Never);
            fx.ProjectService.Verify(s => s.CheckMembershipAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string?>()), Times.Never);
            fx.TaskService.Verify(s => s.GetTaskByIdAsync(It.IsAny<Guid>(), It.IsAny<string?>()), Times.Never);
        }

        // ---------- UpdateTimelogAsync ----------

        [Fact]
        public async Task UpdateTimelogAsync_WithPartialData_OnlyChangesSuppliedFields()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var originalProjectId = Guid.NewGuid();
            var originalTaskId = Guid.NewGuid();
            var loggedByUserId = Guid.NewGuid();
            fx.Context.Timelogs.Add(new Timelog
            {
                Id = id,
                ProjectId = originalProjectId,
                TaskId = originalTaskId,
                HoursSpent = 3,
                Date = new DateTime(2026, 1, 1),
                LoggedByUserId = loggedByUserId
            });
            fx.Context.SaveChanges();
            fx.UserService.Setup(s => s.GetUserInfoAsync(It.IsAny<Guid>())).ReturnsAsync((UserInfoDTO?)null);

            var result = await fx.Repository.UpdateTimelogAsync(id, new TimelogUpdateDTO { HoursSpent = 7.5 }, loggedByUserId, bearerToken: null);

            Assert.NotNull(result);
            Assert.Equal(7.5, result!.HoursSpent);

            var persisted = fx.Context.Timelogs.Single(t => t.Id == id);
            Assert.Equal(7.5, persisted.HoursSpent);
            Assert.Equal(originalProjectId, persisted.ProjectId);
            Assert.Equal(originalTaskId, persisted.TaskId);
        }

        [Fact]
        public async Task UpdateTimelogAsync_ForNonexistentId_ReturnsNullWithoutThrowing()
        {
            var fx = new Fixture();

            var result = await fx.Repository.UpdateTimelogAsync(Guid.NewGuid(), new TimelogUpdateDTO { HoursSpent = 5 }, Guid.NewGuid(), bearerToken: null);

            Assert.Null(result);
            fx.UserService.Verify(s => s.GetUserInfoAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task UpdateTimelogAsync_AlsoFetchesTaskTitle()
        {
            // Reproduces a pre-existing inconsistency found while fixing this: the original
            // code only ever fetched Username on update, never the task/work-package title.
            var fx = new Fixture();
            var id = Guid.NewGuid();
            fx.Context.Timelogs.Add(new Timelog { Id = id, ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid() });
            fx.Context.SaveChanges();
            fx.UserService.Setup(s => s.GetUserInfoAsync(It.IsAny<Guid>())).ReturnsAsync((UserInfoDTO?)null);
            fx.TaskService.Setup(s => s.GetTaskByIdAsync(It.IsAny<Guid>(), It.IsAny<string?>()))
                .ReturnsAsync(new TaskLookupResult(TaskLookupStatus.Found, new TaskDTO { Title = "Updated Task Title", Status = "Done" }));

            var result = await fx.Repository.UpdateTimelogAsync(id, new TimelogUpdateDTO { HoursSpent = 2 }, Guid.Empty, bearerToken: null);

            Assert.Equal("Updated Task Title", result!.TaskTitle);
            Assert.Equal("Done", result.TaskStatus);
        }

        [Fact]
        public async Task UpdateTimelogAsync_WhenNewTaskIdConfirmedNotFound_ThrowsAndDoesNotPersist()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            fx.Context.Timelogs.Add(new Timelog { Id = id, ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid(), HoursSpent = 3 });
            fx.Context.SaveChanges();
            var badTaskId = Guid.NewGuid();
            fx.TaskService.Setup(s => s.GetTaskByIdAsync(badTaskId, It.IsAny<string?>()))
                .ReturnsAsync(new TaskLookupResult(TaskLookupStatus.NotFound, null));

            await Assert.ThrowsAsync<TaskNotFoundException>(
                () => fx.Repository.UpdateTimelogAsync(id, new TimelogUpdateDTO { TaskId = badTaskId }, Guid.Empty, bearerToken: null));

            var persisted = fx.Context.Timelogs.Single(t => t.Id == id);
            Assert.Equal(3, persisted.HoursSpent);
        }

        [Fact]
        public async Task UpdateTimelogAsync_WhenChangedToProjectLoggerIsNotAMemberOf_ThrowsAndDoesNotPersist()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var loggedByUserId = Guid.NewGuid();
            fx.Context.Timelogs.Add(new Timelog { Id = id, ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid(), HoursSpent = 3, LoggedByUserId = loggedByUserId });
            fx.Context.SaveChanges();
            var newProjectId = Guid.NewGuid();
            fx.ProjectService.Setup(s => s.CheckMembershipAsync(newProjectId, loggedByUserId, It.IsAny<string?>()))
                .ReturnsAsync(new ProjectMembershipResult(ProjectMembershipStatus.NotMember));

            await Assert.ThrowsAsync<UserNotProjectMemberException>(
                () => fx.Repository.UpdateTimelogAsync(id, new TimelogUpdateDTO { ProjectId = newProjectId }, loggedByUserId, bearerToken: null));

            var persisted = fx.Context.Timelogs.Single(t => t.Id == id);
            Assert.Equal(3, persisted.HoursSpent);
        }

        [Fact]
        public async Task UpdateTimelogAsync_WhenChangedToProjectThatDoesNotExist_ThrowsAndDoesNotPersist()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            fx.Context.Timelogs.Add(new Timelog { Id = id, ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid(), HoursSpent = 3 });
            fx.Context.SaveChanges();
            var badProjectId = Guid.NewGuid();
            fx.ProjectService.Setup(s => s.CheckProjectExistsAsync(badProjectId, It.IsAny<string?>()))
                .ReturnsAsync(new ProjectExistsResult(ProjectExistsStatus.NotFound));

            await Assert.ThrowsAsync<ProjectNotFoundException>(
                () => fx.Repository.UpdateTimelogAsync(id, new TimelogUpdateDTO { ProjectId = badProjectId }, Guid.Empty, bearerToken: null));

            var persisted = fx.Context.Timelogs.Single(t => t.Id == id);
            Assert.Equal(3, persisted.HoursSpent);
        }

        [Fact]
        public async Task UpdateTimelogAsync_WhenActingUserIsNotOwnerAndNotConfirmedAdmin_ThrowsAndDoesNotPersist()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var loggedByUserId = Guid.NewGuid();
            var actingUserId = Guid.NewGuid();
            fx.Context.Timelogs.Add(new Timelog { Id = id, ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid(), HoursSpent = 3, LoggedByUserId = loggedByUserId });
            fx.Context.SaveChanges();

            await Assert.ThrowsAsync<NotTimelogOwnerException>(
                () => fx.Repository.UpdateTimelogAsync(id, new TimelogUpdateDTO { HoursSpent = 99 }, actingUserId, bearerToken: null));

            var persisted = fx.Context.Timelogs.Single(t => t.Id == id);
            Assert.Equal(3, persisted.HoursSpent);
        }

        [Fact]
        public async Task UpdateTimelogAsync_WhenActingUserIsTheOwner_Succeeds()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var loggedByUserId = Guid.NewGuid();
            fx.Context.Timelogs.Add(new Timelog { Id = id, ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid(), HoursSpent = 3, LoggedByUserId = loggedByUserId });
            fx.Context.SaveChanges();

            var result = await fx.Repository.UpdateTimelogAsync(id, new TimelogUpdateDTO { HoursSpent = 5 }, loggedByUserId, bearerToken: null);

            Assert.NotNull(result);
            Assert.Equal(5, result!.HoursSpent);
        }

        [Fact]
        public async Task UpdateTimelogAsync_WhenActingUserIsConfirmedAdmin_CanUpdateSomeoneElsesTimelog()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var loggedByUserId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            fx.Context.Timelogs.Add(new Timelog { Id = id, ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid(), HoursSpent = 3, LoggedByUserId = loggedByUserId });
            fx.Context.SaveChanges();
            fx.UserService.Setup(s => s.GetUserInfoAsync(adminId))
                .ReturnsAsync(new UserInfoDTO { Username = "admin.user", Role = "Admin" });

            var result = await fx.Repository.UpdateTimelogAsync(id, new TimelogUpdateDTO { HoursSpent = 5 }, adminId, bearerToken: null);

            Assert.NotNull(result);
            Assert.Equal(5, result!.HoursSpent);
        }

        [Fact]
        public async Task UpdateTimelogAsync_WhenActingUserIsOwner_DoesNotCallUserServiceForOwnershipCheck()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var loggedByUserId = Guid.NewGuid();
            fx.Context.Timelogs.Add(new Timelog { Id = id, ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid(), HoursSpent = 3, LoggedByUserId = loggedByUserId });
            fx.Context.SaveChanges();

            await fx.Repository.UpdateTimelogAsync(id, new TimelogUpdateDTO { HoursSpent = 5 }, loggedByUserId, bearerToken: null);

            fx.UserService.Verify(s => s.GetUserInfoAsync(It.IsAny<Guid>()), Times.Once);
        }

        // ---------- DeleteTimelog ----------

        [Fact]
        public async Task DeleteTimelogAsync_WhenActingUserIsTheOwner_RemovesTheRow()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var loggedByUserId = Guid.NewGuid();
            fx.Context.Timelogs.Add(new Timelog { Id = id, ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid(), LoggedByUserId = loggedByUserId });
            fx.Context.SaveChanges();

            await fx.Repository.DeleteTimelogAsync(id, loggedByUserId, bearerToken: null);

            Assert.Empty(fx.Context.Timelogs);
        }

        [Fact]
        public async Task DeleteTimelogAsync_ForNonexistentId_DoesNotThrow()
        {
            var fx = new Fixture();

            var exception = await Record.ExceptionAsync(() => fx.Repository.DeleteTimelogAsync(Guid.NewGuid(), Guid.NewGuid(), bearerToken: null));

            Assert.Null(exception);
        }

        [Fact]
        public async Task DeleteTimelogAsync_WhenActingUserIsNotOwnerAndNotConfirmedAdmin_ThrowsAndDoesNotDelete()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var loggedByUserId = Guid.NewGuid();
            var actingUserId = Guid.NewGuid();
            fx.Context.Timelogs.Add(new Timelog { Id = id, ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid(), LoggedByUserId = loggedByUserId });
            fx.Context.SaveChanges();

            await Assert.ThrowsAsync<NotTimelogOwnerException>(
                () => fx.Repository.DeleteTimelogAsync(id, actingUserId, bearerToken: null));

            Assert.Single(fx.Context.Timelogs);
        }

        [Fact]
        public async Task DeleteTimelogAsync_WhenActingUserIsConfirmedAdmin_CanDeleteSomeoneElsesTimelog()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var loggedByUserId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            fx.Context.Timelogs.Add(new Timelog { Id = id, ProjectId = Guid.NewGuid(), TaskId = Guid.NewGuid(), LoggedByUserId = loggedByUserId });
            fx.Context.SaveChanges();
            fx.UserService.Setup(s => s.GetUserInfoAsync(adminId))
                .ReturnsAsync(new UserInfoDTO { Username = "admin.user", Role = "Admin" });

            await fx.Repository.DeleteTimelogAsync(id, adminId, bearerToken: null);

            Assert.Empty(fx.Context.Timelogs);
        }
    }
}
