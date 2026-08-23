using System;
using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectService.Context;
using ProjectService.Data;
using ProjectService.Models;
using ProjectService.Models.DTO.RequirementsDtos;
using ProjectService.Profiles;
using Xunit;

namespace ProjectService.Tests.Repositories
{
    public class RequirementsRepositoryTests
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
        public void CreateRequirement_ValidData_AddsRequirementAndReturnsConfirmationDto()
        {
            // Arrange
            var context = CreateContext();
            var repository = new RequirementsRepository(context, CreateMapper());

            var projectId = Guid.NewGuid();
            var dto = new RequirementsCreationDto
            {
                ProjectId = projectId,
                Description = "System must support user login"
            };

            // Act
            var result = repository.CreateRequirement(dto);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.RequirementId);
            Assert.Equal(projectId, result.ProjectId);
            Assert.Equal("System must support user login", result.Description);

            var savedRequirement = context.Requirements.FirstOrDefault(r => r.RequirementId == result.RequirementId);
            Assert.NotNull(savedRequirement);
            Assert.Equal("System must support user login", savedRequirement.Description);
        }

        [Fact]
        public void UpdateRequirement_RequirementExists_UpdatesAndReturnsConfirmationDto()
        {
            // Arrange
            var context = CreateContext();
            var existingRequirement = new Requirement
            {
                RequirementId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                Description = "Old description"
            };
            context.Requirements.Add(existingRequirement);
            context.SaveChanges();

            var repository = new RequirementsRepository(context, CreateMapper());

            var newProjectId = Guid.NewGuid();
            var updateDto = new RequirementsUpdateDto
            {
                RequirementId = existingRequirement.RequirementId,
                ProjectId = newProjectId,
                Description = "New description"
            };

            // Act
            var result = repository.UpdateRequirement(updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newProjectId, result!.ProjectId);
            Assert.Equal("New description", result.Description);

            var updatedRequirement = context.Requirements.First(r => r.RequirementId == existingRequirement.RequirementId);
            Assert.Equal("New description", updatedRequirement.Description);
        }

        [Fact]
        public void UpdateRequirement_RequirementDoesNotExist_ReturnsNull()
        {
            // Arrange
            var context = CreateContext();
            var repository = new RequirementsRepository(context, CreateMapper());

            var updateDto = new RequirementsUpdateDto
            {
                RequirementId = Guid.NewGuid(), // ne postoji u bazi
                ProjectId = Guid.NewGuid(),
                Description = "Doesn't matter"
            };

            // Act
            var result = repository.UpdateRequirement(updateDto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void DeleteRequirement_RequirementExists_RemovesRequirementFromDatabase()
        {
            // Arrange
            var context = CreateContext();
            var existingRequirement = new Requirement
            {
                RequirementId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                Description = "To be deleted"
            };
            context.Requirements.Add(existingRequirement);
            context.SaveChanges();

            var repository = new RequirementsRepository(context, CreateMapper());

            // Act
            repository.DeleteRequirement(existingRequirement.RequirementId);

            // Assert
            var deletedRequirement = context.Requirements.FirstOrDefault(r => r.RequirementId == existingRequirement.RequirementId);
            Assert.Null(deletedRequirement);
        }

        [Fact]
        public void GetRequirementById_RequirementExists_ReturnsDto()
        {
            // Arrange
            var context = CreateContext();
            var existingRequirement = new Requirement
            {
                RequirementId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                Description = "Findable requirement"
            };
            context.Requirements.Add(existingRequirement);
            context.SaveChanges();

            var repository = new RequirementsRepository(context, CreateMapper());

            // Act
            var result = repository.GetRequirementById(existingRequirement.RequirementId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingRequirement.RequirementId, result.RequirementId);
            Assert.Equal("Findable requirement", result.Description);
        }

        [Fact]
        public void GetRequirementById_RequirementDoesNotExist_ReturnsNull()
        {
            // Arrange
            var context = CreateContext();
            var repository = new RequirementsRepository(context, CreateMapper());

            // Act
            var result = repository.GetRequirementById(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetRequirementsByProjectId_ReturnsOnlyRequirementsForGivenProject()
        {
            // Arrange
            var context = CreateContext();
            var projectAId = Guid.NewGuid();
            var projectBId = Guid.NewGuid();

            context.Requirements.AddRange(
                new Requirement { RequirementId = Guid.NewGuid(), ProjectId = projectAId, Description = "Req A1" },
                new Requirement { RequirementId = Guid.NewGuid(), ProjectId = projectAId, Description = "Req A2" },
                new Requirement { RequirementId = Guid.NewGuid(), ProjectId = projectBId, Description = "Req B1" }
            );
            context.SaveChanges();

            var repository = new RequirementsRepository(context, CreateMapper());

            // Act
            var result = repository.GetRequirementsByProjectId(projectAId).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal(projectAId, r.ProjectId));
        }
    }
}
