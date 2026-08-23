using Moq;
using WorkPackageService.Context;
using WorkPackageService.Data;
using WorkPackageService.Exceptions;
using WorkPackageService.Models.DTO.TaskDTOs;
using WorkPackageService.ServiceCalls.Notification;
using WorkPackageService.Tests.TestHelpers;
using DependencyEntity = WorkPackageService.Models.Dependency;
using TaskEntity = WorkPackageService.Models.Task;
using TaskPriority = WorkPackageService.Models.Enums.TaskPriority;
using TaskStatus = WorkPackageService.Models.Enums.TaskStatus;

namespace WorkPackageService.Tests.RepositoryTests
{
    public class TaskRepositoryTests
    {
        private static TaskRepository CreateRepository(
            WorkPackageServiceContext context,
            out Mock<INotificationService> notificationServiceMock)
        {
            notificationServiceMock = new Mock<INotificationService>();
            notificationServiceMock
                .Setup(s => s.SendNotificationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            var mapper = DbContextFactory.CreateMapper();
            return new TaskRepository(context, mapper, notificationServiceMock.Object);
        }

        [Fact]
        public async Task UpdateStatus_WhenCallerIsAssignee_UpdatesStatus()
        {
            // Arrange
            using var context = DbContextFactory.CreateContext();
            var assigneeId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            context.Tasks.Add(new TaskEntity
            {
                TaskId = taskId,
                WorkPackageId = Guid.NewGuid(),
                Title = "Test task",
                Status = TaskStatus.ToDo,
                Priority = TaskPriority.Medium,
                AssigneeId = assigneeId,
                CreatedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var repository = CreateRepository(context, out _);

            // Act
            var result = await repository.UpdateStatus(taskId, assigneeId, TaskStatus.InProgress);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TaskStatus.InProgress, result!.Status);
        }

        [Fact]
        public async Task UpdateStatus_WhenCallerIsNotAssignee_ThrowsUnauthorizedOperationException()
        {
            // Arrange
            using var context = DbContextFactory.CreateContext();
            var assigneeId = Guid.NewGuid();
            var wrongCallerId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            context.Tasks.Add(new TaskEntity
            {
                TaskId = taskId,
                WorkPackageId = Guid.NewGuid(),
                Title = "Test task",
                Status = TaskStatus.ToDo,
                Priority = TaskPriority.Medium,
                AssigneeId = assigneeId,
                CreatedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var repository = CreateRepository(context, out _);

            // Act + Assert
            await Assert.ThrowsAsync<UnauthorizedOperationException>(
                () => repository.UpdateStatus(taskId, wrongCallerId, TaskStatus.Done));
        }

        [Fact]
        public async Task UpdateStatus_WhenTaskDoesNotExist_ThrowsEntityNotFoundException()
        {
            // Arrange
            using var context = DbContextFactory.CreateContext();
            var repository = CreateRepository(context, out _);

            // Act + Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => repository.UpdateStatus(Guid.NewGuid(), Guid.NewGuid(), TaskStatus.Done));
        }

        [Fact]
        public async Task UpdateStatus_WhenStatusBecomesDone_SendsNotificationToBlockedTaskAssignees()
        {
            // Arrange
            using var context = DbContextFactory.CreateContext();
            var blockerAssigneeId = Guid.NewGuid();
            var blockedAssigneeId = Guid.NewGuid();
            var workPackageId = Guid.NewGuid();
            var blockerTaskId = Guid.NewGuid();
            var blockedTaskId = Guid.NewGuid();

            context.Tasks.Add(new TaskEntity
            {
                TaskId = blockerTaskId,
                WorkPackageId = workPackageId,
                Title = "Blocker",
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.Medium,
                AssigneeId = blockerAssigneeId,
                CreatedAt = DateTime.UtcNow
            });
            context.Tasks.Add(new TaskEntity
            {
                TaskId = blockedTaskId,
                WorkPackageId = workPackageId,
                Title = "Blocked",
                Status = TaskStatus.ToDo,
                Priority = TaskPriority.Medium,
                AssigneeId = blockedAssigneeId,
                CreatedAt = DateTime.UtcNow
            });
            context.Dependencies.Add(new DependencyEntity
            {
                DependencyId = Guid.NewGuid(),
                TaskId = blockedTaskId,
                BlockerTaskId = blockerTaskId,
                CreatedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var repository = CreateRepository(context, out var notificationServiceMock);

            // Act
            await repository.UpdateStatus(blockerTaskId, blockerAssigneeId, TaskStatus.Done);

            // Assert
            notificationServiceMock.Verify(
                s => s.SendNotificationAsync(blockedAssigneeId, It.IsAny<string>(), "TaskUnblocked"),
                Times.Once);
        }

        [Fact]
        public void Delete_WhenTaskHasDependencies_RemovesDependenciesToo()
        {
            // Arrange
            using var context = DbContextFactory.CreateContext();
            var workPackageId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var otherTaskId = Guid.NewGuid();

            context.Tasks.Add(new TaskEntity { TaskId = taskId, WorkPackageId = workPackageId, Title = "T1", Status = TaskStatus.ToDo, Priority = TaskPriority.Low, CreatedAt = DateTime.UtcNow });
            context.Tasks.Add(new TaskEntity { TaskId = otherTaskId, WorkPackageId = workPackageId, Title = "T2", Status = TaskStatus.ToDo, Priority = TaskPriority.Low, CreatedAt = DateTime.UtcNow });
            context.Dependencies.Add(new DependencyEntity { DependencyId = Guid.NewGuid(), TaskId = taskId, BlockerTaskId = otherTaskId, CreatedAt = DateTime.UtcNow });
            context.Dependencies.Add(new DependencyEntity { DependencyId = Guid.NewGuid(), TaskId = otherTaskId, BlockerTaskId = taskId, CreatedAt = DateTime.UtcNow });
            context.SaveChanges();

            var repository = CreateRepository(context, out _);

            // Act
            var deleted = repository.Delete(taskId);

            // Assert
            Assert.True(deleted);
            Assert.Empty(context.Dependencies.Where(d => d.TaskId == taskId || d.BlockerTaskId == taskId));
        }

        [Fact]
        public void MoveToWorkPackage_WhenTaskHasDependencies_ReturnsWarning()
        {
            // Arrange
            using var context = DbContextFactory.CreateContext();
            var taskId = Guid.NewGuid();
            var blockerTaskId = Guid.NewGuid();
            var newWorkPackageId = Guid.NewGuid();

            context.Tasks.Add(new TaskEntity { TaskId = taskId, WorkPackageId = Guid.NewGuid(), Title = "T1", Status = TaskStatus.ToDo, Priority = TaskPriority.Low, CreatedAt = DateTime.UtcNow });
            context.Dependencies.Add(new DependencyEntity { DependencyId = Guid.NewGuid(), TaskId = taskId, BlockerTaskId = blockerTaskId, CreatedAt = DateTime.UtcNow });
            context.SaveChanges();

            var repository = CreateRepository(context, out _);

            // Act
            var result = repository.MoveToWorkPackage(taskId, newWorkPackageId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result!.HasDependencyWarning);
            Assert.NotNull(result.Warning);
            Assert.Equal(newWorkPackageId, result.Task.WorkPackageId);
        }

        [Fact]
        public void MoveToWorkPackage_WhenTaskHasNoDependencies_NoWarning()
        {
            // Arrange
            using var context = DbContextFactory.CreateContext();
            var taskId = Guid.NewGuid();
            var newWorkPackageId = Guid.NewGuid();

            context.Tasks.Add(new TaskEntity { TaskId = taskId, WorkPackageId = Guid.NewGuid(), Title = "T1", Status = TaskStatus.ToDo, Priority = TaskPriority.Low, CreatedAt = DateTime.UtcNow });
            context.SaveChanges();

            var repository = CreateRepository(context, out _);

            // Act
            var result = repository.MoveToWorkPackage(taskId, newWorkPackageId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result!.HasDependencyWarning);
            Assert.Null(result.Warning);
        }

        [Fact]
        public async Task Reassign_SendsNotificationToOldAndNewAssignee()
        {
            // Arrange
            using var context = DbContextFactory.CreateContext();
            var oldAssigneeId = Guid.NewGuid();
            var newAssigneeId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            context.Tasks.Add(new TaskEntity
            {
                TaskId = taskId,
                WorkPackageId = Guid.NewGuid(),
                Title = "T1",
                Status = TaskStatus.ToDo,
                Priority = TaskPriority.Low,
                AssigneeId = oldAssigneeId,
                CreatedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var repository = CreateRepository(context, out var notificationServiceMock);

            // Act
            var result = await repository.Reassign(taskId, newAssigneeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(oldAssigneeId, result!.OldAssigneeId);
            Assert.Equal(newAssigneeId, result.NewAssigneeId);
            notificationServiceMock.Verify(
                s => s.SendNotificationAsync(oldAssigneeId, It.IsAny<string>(), "TaskReassignedFrom"),
                Times.Once);
            notificationServiceMock.Verify(
                s => s.SendNotificationAsync(newAssigneeId, It.IsAny<string>(), "TaskReassignedTo"),
                Times.Once);
        }

        // Regresioni testovi za AutoMapper partial-update bag (nullable enum -> non-nullable enum
        // se tiho resetovao na default umesto da se preskoci kad polje nije prosledjeno).
        [Fact]
        public void Update_WhenStatusNotProvided_PreservesExistingStatus()
        {
            // Arrange
            using var context = DbContextFactory.CreateContext();
            var taskId = Guid.NewGuid();

            context.Tasks.Add(new TaskEntity
            {
                TaskId = taskId,
                WorkPackageId = Guid.NewGuid(),
                Title = "Original title",
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.High,
                CreatedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var repository = CreateRepository(context, out _);
            var updateDto = new TaskUpdateDTO { Id = taskId, Title = "New title" };

            // Act
            var result = repository.Update(taskId, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New title", result!.Title);
            Assert.Equal(TaskStatus.InProgress, result.Status);
        }

        [Fact]
        public void Update_WhenPriorityNotProvided_PreservesExistingPriority()
        {
            // Arrange
            using var context = DbContextFactory.CreateContext();
            var taskId = Guid.NewGuid();

            context.Tasks.Add(new TaskEntity
            {
                TaskId = taskId,
                WorkPackageId = Guid.NewGuid(),
                Title = "Original title",
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.High,
                CreatedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var repository = CreateRepository(context, out _);
            var updateDto = new TaskUpdateDTO { Id = taskId, Title = "New title" };

            // Act
            var result = repository.Update(taskId, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TaskPriority.High, result!.Priority);
        }
    }
}
