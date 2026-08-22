using System;
using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectService.Context;
using ProjectService.Data;
using ProjectService.Models;
using ProjectService.Models.DTO.MilestoneDtos;
using ProjectService.Profiles;
using Xunit;

namespace ProjectService.Tests.Repositories
{
    public class MilestoneRepositoryTests
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
        public void CreateMilestone_ValidData_AddsMilestoneAndReturnsConfirmationDto()
        {
            // Arrange
            var context = CreateContext();
            var repository = new MilestoneRepository(context, CreateMapper());

            var projectId = Guid.NewGuid();
            var dto = new MilestoneCreationDto
            {
                ProjectId = projectId,
                ExpectedDate = DateTime.Now.AddMonths(1)
            };

            // Act
            var result = repository.CreateMilestone(dto);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.MilestoneId);
            Assert.Equal(projectId, result.ProjectId);

            var savedMilestone = context.Milestones.FirstOrDefault(m => m.MilestoneId == result.MilestoneId);
            Assert.NotNull(savedMilestone);
            Assert.Equal(projectId, savedMilestone.ProjectId);
        }

        [Fact]
        public void UpdateMilestone_MilestoneExists_UpdatesAndReturnsConfirmationDto()
        {
            // Arrange
            var context = CreateContext();
            var existingMilestone = new Milestone
            {
                MilestoneId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                ExpectedDate = DateTime.Now.AddDays(10)
            };
            context.Milestones.Add(existingMilestone);
            context.SaveChanges();

            var repository = new MilestoneRepository(context, CreateMapper());

            var newProjectId = Guid.NewGuid();
            var newDate = DateTime.Now.AddMonths(3);
            var updateDto = new MilestoneUpdateDto
            {
                MilestoneId = existingMilestone.MilestoneId,
                ProjectId = newProjectId,
                ExpectedDate = newDate
            };

            // Act
            var result = repository.UpdateMilestone(updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newProjectId, result!.ProjectId);
            Assert.Equal(newDate, result.ExpectedDate);

            var updatedMilestone = context.Milestones.First(m => m.MilestoneId == existingMilestone.MilestoneId);
            Assert.Equal(newProjectId, updatedMilestone.ProjectId);
        }

        [Fact]
        public void UpdateMilestone_MilestoneDoesNotExist_ReturnsNull()
        {
            // Arrange
            var context = CreateContext();
            var repository = new MilestoneRepository(context, CreateMapper());

            var updateDto = new MilestoneUpdateDto
            {
                MilestoneId = Guid.NewGuid(), // ne postoji u bazi
                ProjectId = Guid.NewGuid(),
                ExpectedDate = DateTime.Now.AddDays(5)
            };

            // Act
            var result = repository.UpdateMilestone(updateDto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void DeleteMilestone_MilestoneExists_RemovesMilestoneFromDatabase()
        {
            // Arrange
            var context = CreateContext();
            var existingMilestone = new Milestone
            {
                MilestoneId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                ExpectedDate = DateTime.Now.AddDays(20)
            };
            context.Milestones.Add(existingMilestone);
            context.SaveChanges();

            var repository = new MilestoneRepository(context, CreateMapper());

            // Act
            repository.DeleteMilestone(existingMilestone.MilestoneId);

            // Assert
            var deletedMilestone = context.Milestones.FirstOrDefault(m => m.MilestoneId == existingMilestone.MilestoneId);
            Assert.Null(deletedMilestone);
        }

        [Fact]
        public void GetMilestoneById_MilestoneExists_ReturnsDto()
        {
            // Arrange
            var context = CreateContext();
            var existingMilestone = new Milestone
            {
                MilestoneId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                ExpectedDate = DateTime.Now.AddDays(15)
            };
            context.Milestones.Add(existingMilestone);
            context.SaveChanges();

            var repository = new MilestoneRepository(context, CreateMapper());

            // Act
            var result = repository.GetMilestoneById(existingMilestone.MilestoneId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingMilestone.MilestoneId, result.MilestoneId);
            Assert.Equal(existingMilestone.ProjectId, result.ProjectId);
        }

        [Fact]
        public void GetMilestoneById_MilestoneDoesNotExist_ReturnsNull()
        {
            // Arrange
            var context = CreateContext();
            var repository = new MilestoneRepository(context, CreateMapper());

            // Act
            var result = repository.GetMilestoneById(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetMilestonesByProjectId_ReturnsOnlyMilestonesForGivenProject()
        {
            // Arrange
            var context = CreateContext();
            var projectAId = Guid.NewGuid();
            var projectBId = Guid.NewGuid();

            context.Milestones.AddRange(
                new Milestone { MilestoneId = Guid.NewGuid(), ProjectId = projectAId, ExpectedDate = DateTime.Now.AddDays(5) },
                new Milestone { MilestoneId = Guid.NewGuid(), ProjectId = projectAId, ExpectedDate = DateTime.Now.AddDays(10) },
                new Milestone { MilestoneId = Guid.NewGuid(), ProjectId = projectBId, ExpectedDate = DateTime.Now.AddDays(15) }
            );
            context.SaveChanges();

            var repository = new MilestoneRepository(context, CreateMapper());

            // Act
            var result = repository.GetMilestonesByProjectId(projectAId).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, m => Assert.Equal(projectAId, m.ProjectId));
        }
    }
}
