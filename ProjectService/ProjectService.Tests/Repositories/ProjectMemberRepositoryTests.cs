using System;
using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectService.Context;
using ProjectService.Data;
using ProjectService.Models;
using ProjectService.Models.DTO.ProjectMemberDtos;
using ProjectService.Profiles;
using Xunit;

namespace ProjectService.Tests.Repositories
{
    public class ProjectMemberRepositoryTests
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
        public void CreateProjectMember_ValidData_AddsMemberAndReturnsConfirmationDto()
        {
            // Arrange
            var context = CreateContext();
            var repository = new ProjectMemberRepository(context, CreateMapper());

            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var dto = new ProjectMemberCreationDto
            {
                ProjectId = projectId,
                UserId = userId
            };

            // Act
            var result = repository.CreateProjectMember(dto);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.ProjectMemberId);
            Assert.Equal(projectId, result.ProjectId);
            Assert.Equal(userId, result.UserId);
            Assert.True(result.Status);

            var savedMember = context.ProjectMembers.FirstOrDefault(pm => pm.ProjectMemberId == result.ProjectMemberId);
            Assert.NotNull(savedMember);
            Assert.Equal(projectId, savedMember.ProjectId);
            Assert.Equal(userId, savedMember.UserId);
        }

        [Fact]
        public void UpdateProjectMember_MemberExists_UpdatesAndReturnsConfirmationDto()
        {
            // Arrange
            var context = CreateContext();
            var existingMember = new ProjectMember
            {
                ProjectMemberId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                JoinedAt = DateTime.UtcNow.AddDays(-5),
                Status = true
            };
            context.ProjectMembers.Add(existingMember);
            context.SaveChanges();

            var repository = new ProjectMemberRepository(context, CreateMapper());

            var newProjectId = Guid.NewGuid();
            var updateDto = new ProjectMemberUpdateDto
            {
                ProjectMemberId = existingMember.ProjectMemberId,
                ProjectId = newProjectId,
                UserId = existingMember.UserId,
                JoinedAt = existingMember.JoinedAt,
                Status = false
            };

            // Act
            var result = repository.UpdateProjectMember(updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newProjectId, result!.ProjectId);
            Assert.False(result.Status);

            var updatedMember = context.ProjectMembers.First(pm => pm.ProjectMemberId == existingMember.ProjectMemberId);
            Assert.Equal(newProjectId, updatedMember.ProjectId);
            Assert.False(updatedMember.Status);
        }

        [Fact]
        public void UpdateProjectMember_MemberDoesNotExist_ReturnsNull()
        {
            // Arrange
            var context = CreateContext();
            var repository = new ProjectMemberRepository(context, CreateMapper());

            var updateDto = new ProjectMemberUpdateDto
            {
                ProjectMemberId = Guid.NewGuid(), // ne postoji u bazi
                ProjectId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                JoinedAt = DateTime.UtcNow,
                Status = true
            };

            // Act
            var result = repository.UpdateProjectMember(updateDto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void DeleteProjectMember_MemberExists_RemovesMemberFromDatabase()
        {
            // Arrange
            var context = CreateContext();
            var existingMember = new ProjectMember
            {
                ProjectMemberId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                JoinedAt = DateTime.UtcNow,
                Status = true
            };
            context.ProjectMembers.Add(existingMember);
            context.SaveChanges();

            var repository = new ProjectMemberRepository(context, CreateMapper());

            // Act
            repository.DeleteProjectMember(existingMember.ProjectMemberId);

            // Assert
            var deletedMember = context.ProjectMembers.FirstOrDefault(pm => pm.ProjectMemberId == existingMember.ProjectMemberId);
            Assert.Null(deletedMember);
        }

        [Fact]
        public void GetProjectMemberById_MemberExists_ReturnsDto()
        {
            // Arrange
            var context = CreateContext();
            var existingMember = new ProjectMember
            {
                ProjectMemberId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                JoinedAt = DateTime.UtcNow,
                Status = true
            };
            context.ProjectMembers.Add(existingMember);
            context.SaveChanges();

            var repository = new ProjectMemberRepository(context, CreateMapper());

            // Act
            var result = repository.GetProjectMemberById(existingMember.ProjectMemberId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingMember.ProjectMemberId, result.ProjectMemberId);
            Assert.Equal(existingMember.UserId, result.UserId);
        }

        [Fact]
        public void GetProjectMemberById_MemberDoesNotExist_ReturnsNull()
        {
            // Arrange
            var context = CreateContext();
            var repository = new ProjectMemberRepository(context, CreateMapper());

            // Act
            var result = repository.GetProjectMemberById(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetMembersByProjectId_ReturnsOnlyMembersForGivenProject()
        {
            // Arrange
            var context = CreateContext();
            var projectAId = Guid.NewGuid();
            var projectBId = Guid.NewGuid();

            context.ProjectMembers.AddRange(
                new ProjectMember { ProjectMemberId = Guid.NewGuid(), ProjectId = projectAId, UserId = Guid.NewGuid(), JoinedAt = DateTime.UtcNow, Status = true },
                new ProjectMember { ProjectMemberId = Guid.NewGuid(), ProjectId = projectAId, UserId = Guid.NewGuid(), JoinedAt = DateTime.UtcNow, Status = true },
                new ProjectMember { ProjectMemberId = Guid.NewGuid(), ProjectId = projectBId, UserId = Guid.NewGuid(), JoinedAt = DateTime.UtcNow, Status = true }
            );
            context.SaveChanges();

            var repository = new ProjectMemberRepository(context, CreateMapper());

            // Act
            var result = repository.GetMembersByProjectId(projectAId).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, pm => Assert.Equal(projectAId, pm.ProjectId));
        }
    }
}
