using System;
using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectService.Context;
using ProjectService.Data;
using ProjectService.Models;
using ProjectService.Models.DTO.ProjectDtos;
using ProjectService.Models.Enums;
using ProjectService.Profiles;
using Xunit;

namespace ProjectService.Tests.Repositories
{
    public class ProjectRepositoryTests
    {
        private static ProjectContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ProjectContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var configuration = new ConfigurationBuilder().Build();

            return new ProjectContext(options, configuration);
        }

        private static IMapper CreateMapper()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAutoMapper(cfg => cfg.AddMaps(typeof(ProjectProfile).Assembly));
            return services.BuildServiceProvider().GetRequiredService<IMapper>();
        }

        [Fact]
        public void CreateProject_ValidData_AddsProjectAndReturnsConfirmationDto()
        {
            // Arrange
            var context = CreateContext();
            var repository = new ProjectRepository(context, CreateMapper());

            var dto = new ProjectCreationDto
            {
                Name = "Test Project",
                Budget = 5000,
                Status = ProjectStatus.Planned,
                Deadline = DateTime.Now.AddMonths(1)
            };

            // Act
            var result = repository.CreateProject(dto);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.ProjectId);
            Assert.Equal("Test Project", result.Name);
            Assert.Equal(5000, result.Budget);
            Assert.Equal(ProjectStatus.Planned, result.Status);

            var savedProject = context.Projects.FirstOrDefault(p => p.ProjectId == result.ProjectId);
            Assert.NotNull(savedProject);
            Assert.Equal("Test Project", savedProject.Name);
        }

        [Fact]
        public void UpdateProject_ProjectExists_UpdatesAndReturnsConfirmationDto()
        {
            // Arrange
            var context = CreateContext();
            var existingProject = new Project
            {
                ProjectId = Guid.NewGuid(),
                Name = "Old Name",
                Budget = 1000,
                Status = ProjectStatus.Planned,
                Deadline = null,
                CreatedAt = DateTime.UtcNow
            };
            context.Projects.Add(existingProject);
            context.SaveChanges();

            var repository = new ProjectRepository(context, CreateMapper());

            var updateDto = new ProjectUpdateDto
            {
                ProjectId = existingProject.ProjectId,
                Name = "New Name",
                Budget = 2000,
                Status = ProjectStatus.Active,
                Deadline = DateTime.Now.AddMonths(2)
            };

            // Act
            var result = repository.UpdateProject(updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Name", result!.Name);
            Assert.Equal(2000, result.Budget);
            Assert.Equal(ProjectStatus.Active, result.Status);

            var updatedProject = context.Projects.First(p => p.ProjectId == existingProject.ProjectId);
            Assert.Equal("New Name", updatedProject.Name);
            Assert.Equal(2000, updatedProject.Budget);
        }

        [Fact]
        public void UpdateProject_ProjectDoesNotExist_ReturnsNull()
        {
            // Arrange
            var context = CreateContext();
            var repository = new ProjectRepository(context, CreateMapper());

            var updateDto = new ProjectUpdateDto
            {
                ProjectId = Guid.NewGuid(), // ne postoji u bazi
                Name = "Doesn't matter",
                Budget = 100,
                Status = ProjectStatus.Planned,
                Deadline = null
            };

            // Act
            var result = repository.UpdateProject(updateDto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void DeleteProject_ProjectExists_RemovesProjectFromDatabase()
        {
            // Arrange
            var context = CreateContext();
            var existingProject = new Project
            {
                ProjectId = Guid.NewGuid(),
                Name = "To Delete",
                Budget = 500,
                Status = ProjectStatus.Planned,
                CreatedAt = DateTime.UtcNow
            };
            context.Projects.Add(existingProject);
            context.SaveChanges();

            var repository = new ProjectRepository(context, CreateMapper());

            // Act
            repository.DeleteProject(existingProject.ProjectId);

            // Assert
            var deletedProject = context.Projects.FirstOrDefault(p => p.ProjectId == existingProject.ProjectId);
            Assert.Null(deletedProject);
        }

        [Fact]
        public void GetProjectById_ProjectExists_ReturnsDto()
        {
            // Arrange
            var context = CreateContext();
            var existingProject = new Project
            {
                ProjectId = Guid.NewGuid(),
                Name = "Findable Project",
                Budget = 750,
                Status = ProjectStatus.Active,
                CreatedAt = DateTime.UtcNow
            };
            context.Projects.Add(existingProject);
            context.SaveChanges();

            var repository = new ProjectRepository(context, CreateMapper());

            // Act
            var result = repository.GetProjectById(existingProject.ProjectId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingProject.ProjectId, result.ProjectId);
            Assert.Equal("Findable Project", result.Name);
        }

        [Fact]
        public void GetProjectById_ProjectDoesNotExist_ReturnsNull()
        {
            // Arrange
            var context = CreateContext();
            var repository = new ProjectRepository(context, CreateMapper());

            // Act
            var result = repository.GetProjectById(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetProjectsByStatus_ReturnsOnlyProjectsWithMatchingStatus()
        {
            // Arrange
            var context = CreateContext();
            context.Projects.AddRange(
                new Project { ProjectId = Guid.NewGuid(), Name = "Active 1", Budget = 100, Status = ProjectStatus.Active, CreatedAt = DateTime.UtcNow },
                new Project { ProjectId = Guid.NewGuid(), Name = "Active 2", Budget = 200, Status = ProjectStatus.Active, CreatedAt = DateTime.UtcNow },
                new Project { ProjectId = Guid.NewGuid(), Name = "Completed 1", Budget = 300, Status = ProjectStatus.Completed, CreatedAt = DateTime.UtcNow }
            );
            context.SaveChanges();

            var repository = new ProjectRepository(context, CreateMapper());

            // Act
            var result = repository.GetProjectsByStatus(ProjectStatus.Active).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal(ProjectStatus.Active, p.Status));
        }

        [Fact]
        public void GetProjectsByUserId_ReturnsOnlyProjectsForGivenUser()
        {
            // Arrange
            var context = CreateContext();

            var userAId = Guid.NewGuid();
            var userBId = Guid.NewGuid();

            var projectA = new Project { ProjectId = Guid.NewGuid(), Name = "User A Project", Budget = 100, Status = ProjectStatus.Active, CreatedAt = DateTime.UtcNow };
            var projectB = new Project { ProjectId = Guid.NewGuid(), Name = "User B Project", Budget = 200, Status = ProjectStatus.Active, CreatedAt = DateTime.UtcNow };
            context.Projects.AddRange(projectA, projectB);

            context.ProjectMembers.AddRange(
                new ProjectMember { ProjectMemberId = Guid.NewGuid(), ProjectId = projectA.ProjectId, UserId = userAId, JoinedAt = DateTime.UtcNow, Status = true },
                new ProjectMember { ProjectMemberId = Guid.NewGuid(), ProjectId = projectB.ProjectId, UserId = userBId, JoinedAt = DateTime.UtcNow, Status = true }
            );
            context.SaveChanges();

            var repository = new ProjectRepository(context, CreateMapper());

            // Act
            var result = repository.GetProjectsByUserId(userAId).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(projectA.ProjectId, result[0].ProjectId);
        }
    }
}
