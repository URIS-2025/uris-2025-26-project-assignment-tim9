using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SprintService.Context;
using SprintService.Data;
using SprintService.Models;
using SprintService.Models.DTO;
using SprintService.Models.DTO.Project;
using SprintService.Models.Enums;
using SprintService.Profiles;
using SprintService.ServiceCalls.Project;

namespace SprintService.Tests
{
    public class SprintRepositoryTests
    {
        private static SprintContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<SprintContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var configuration = new ConfigurationBuilder().Build();

            return new SprintContext(options, configuration);
        }

        private static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<SprintProfile>(), NullLoggerFactory.Instance);
            return config.CreateMapper();
        }

        private sealed class Fixture
        {
            public SprintContext Context { get; }
            public SprintRepository Repository { get; }
            public Mock<IProjectService> ProjectService { get; }

            public Fixture()
            {
                Context = CreateContext();
                ProjectService = new Mock<IProjectService>();
                Repository = new SprintRepository(Context, CreateMapper(), ProjectService.Object);
            }

            /// <summary>Stubs the project-existence check to confirm this project is real.</summary>
            public void ProjectExists(Guid projectId) =>
                ProjectService.Setup(s => s.CheckProjectExistsAsync(projectId)).ReturnsAsync(ProjectExistence.Exists);
        }

        // ---------- GetSprints ----------

        [Fact]
        public void GetSprints_WithNoFilter_ReturnsAll()
        {
            var fx = new Fixture();
            fx.Context.Sprints.AddRange(
                new Sprint { Id = Guid.NewGuid(), Name = "A", ProjectId = Guid.NewGuid() },
                new Sprint { Id = Guid.NewGuid(), Name = "B", ProjectId = Guid.NewGuid() }
            );
            fx.Context.SaveChanges();

            var result = fx.Repository.GetSprints();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void GetSprints_FilteredByProjectId_ReturnsOnlyThatProject()
        {
            var fx = new Fixture();
            var projectId = Guid.NewGuid();
            fx.Context.Sprints.AddRange(
                new Sprint { Id = Guid.NewGuid(), Name = "In project", ProjectId = projectId },
                new Sprint { Id = Guid.NewGuid(), Name = "Other project", ProjectId = Guid.NewGuid() }
            );
            fx.Context.SaveChanges();

            var result = fx.Repository.GetSprints(projectId).ToList();

            Assert.Single(result);
            Assert.Equal("In project", result[0].Name);
        }

        // ---------- GetSprintsForCallerAsync ----------

        [Fact]
        public async Task GetSprintsForCallerAsync_WithNoProjectIdFilter_DoesNotCheckProjectExistence()
        {
            // GET /sprints with no filter isn't scoped to any one project, so there's nothing to
            // confirm exists.
            var fx = new Fixture();

            var result = await fx.Repository.GetSprintsForCallerAsync(null, null);

            Assert.Empty(result);
            fx.ProjectService.Verify(s => s.CheckProjectExistsAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetSprintsForCallerAsync_WithProjectIdFilter_ReturnsMatchingSprintsWhenProjectExists()
        {
            var fx = new Fixture();
            var projectId = Guid.NewGuid();
            fx.ProjectExists(projectId);
            fx.Context.Sprints.AddRange(
                new Sprint { Id = Guid.NewGuid(), Name = "In project", ProjectId = projectId },
                new Sprint { Id = Guid.NewGuid(), Name = "Other project", ProjectId = Guid.NewGuid() }
            );
            fx.Context.SaveChanges();

            var result = (await fx.Repository.GetSprintsForCallerAsync(projectId, null)).ToList();

            Assert.Single(result);
            Assert.Equal("In project", result[0].Name);
        }

        [Fact]
        public async Task GetSprintsForCallerAsync_WhenProjectDoesNotExist_ThrowsProjectNotFoundException()
        {
            var fx = new Fixture();
            var projectId = Guid.NewGuid();
            fx.ProjectService.Setup(s => s.CheckProjectExistsAsync(projectId)).ReturnsAsync(ProjectExistence.NotFound);

            await Assert.ThrowsAsync<ProjectNotFoundException>(() => fx.Repository.GetSprintsForCallerAsync(projectId, null));
        }

        [Fact]
        public async Task GetSprintsForCallerAsync_WhenProjectExistenceIsUnknown_ThrowsSprintValidationException()
        {
            var fx = new Fixture();
            var projectId = Guid.NewGuid();
            fx.ProjectService.Setup(s => s.CheckProjectExistsAsync(projectId)).ReturnsAsync(ProjectExistence.Unknown);

            await Assert.ThrowsAsync<SprintValidationException>(() => fx.Repository.GetSprintsForCallerAsync(projectId, null));
        }

        // ---------- GetSprintById ----------

        [Fact]
        public void GetSprintById_ForExisting_ReturnsIt()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            fx.Context.Sprints.Add(new Sprint { Id = id, Name = "A", ProjectId = Guid.NewGuid() });
            fx.Context.SaveChanges();

            var result = fx.Repository.GetSprintById(id);

            Assert.NotNull(result);
            Assert.Equal(id, result!.Id);
        }

        [Fact]
        public void GetSprintById_ForNonexistent_ReturnsNull()
        {
            var fx = new Fixture();

            var result = fx.Repository.GetSprintById(Guid.NewGuid());

            Assert.Null(result);
        }

        // ---------- CreateSprintAsync ----------

        [Fact]
        public async Task CreateSprintAsync_PersistsAndReturnsConfirmation()
        {
            var fx = new Fixture();
            var projectId = Guid.NewGuid();
            fx.ProjectExists(projectId);
            fx.ProjectService.Setup(s => s.GetProjectByIdAsync(projectId)).ReturnsAsync((MilestoneDTO?)null);
            var dto = new SprintCreationDTO
            {
                Name = "New Sprint",
                Status = SprintStatus.NotStarted,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 1, 15)
            };

            var result = await fx.Repository.CreateSprintAsync(projectId, dto);

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal("New Sprint", result.Name);
            var persisted = fx.Context.Sprints.Single(s => s.Id == result.Id);
            Assert.Equal(projectId, persisted.ProjectId);
        }

        [Fact]
        public async Task CreateSprintAsync_WhenProjectServiceHasData_EnrichesWithMilestoneInfo()
        {
            var fx = new Fixture();
            var projectId = Guid.NewGuid();
            var milestoneId = Guid.NewGuid();
            var expectedDate = new DateTime(2026, 3, 1);
            fx.ProjectExists(projectId);
            fx.ProjectService.Setup(s => s.GetProjectByIdAsync(projectId))
                .ReturnsAsync(new MilestoneDTO { MilestoneID = milestoneId, ExpectedDate = expectedDate });
            var dto = new SprintCreationDTO
            {
                Name = "Enriched Sprint",
                Status = SprintStatus.NotStarted,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 1, 15)
            };

            var result = await fx.Repository.CreateSprintAsync(projectId, dto);

            Assert.Equal(milestoneId, result.MilestoneId);
            Assert.Equal(expectedDate, result.ExpectedDate);
        }

        [Fact]
        public async Task CreateSprintAsync_WhenProjectServiceReturnsNull_StillSucceedsWithoutEnrichment()
        {
            // Reproduces the real bug found during manual testing: an unreachable Project
            // Service must not prevent sprint creation from succeeding - it should only skip
            // the milestone enrichment. IProjectService.GetProjectByIdAsync returning null
            // (its documented contract for "unreachable/unknown") must not throw here.
            var fx = new Fixture();
            var projectId = Guid.NewGuid();
            fx.ProjectExists(projectId);
            fx.ProjectService.Setup(s => s.GetProjectByIdAsync(projectId)).ReturnsAsync((MilestoneDTO?)null);
            var dto = new SprintCreationDTO
            {
                Name = "Resilient Sprint",
                Status = SprintStatus.NotStarted,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 1, 15)
            };

            var result = await fx.Repository.CreateSprintAsync(projectId, dto);

            Assert.Equal("Resilient Sprint", result.Name);
            Assert.Null(result.MilestoneId);
            Assert.Null(result.ExpectedDate);
            Assert.Single(fx.Context.Sprints);
        }

        [Fact]
        public async Task CreateSprintAsync_WithEndDateAfterMilestoneExpectedDate_ThrowsAndPersistsNothing()
        {
            var fx = new Fixture();
            var projectId = Guid.NewGuid();
            fx.ProjectExists(projectId);
            fx.ProjectService.Setup(s => s.GetProjectByIdAsync(projectId))
                .ReturnsAsync(new MilestoneDTO { MilestoneID = Guid.NewGuid(), ExpectedDate = new DateTime(2026, 1, 10) });
            var dto = new SprintCreationDTO
            {
                Name = "Too Late Sprint",
                Status = SprintStatus.NotStarted,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 1, 15) // after the milestone's 2026-01-10 due date
            };

            await Assert.ThrowsAsync<SprintValidationException>(() => fx.Repository.CreateSprintAsync(projectId, dto));
            Assert.Empty(fx.Context.Sprints);
        }

        [Fact]
        public async Task CreateSprintAsync_WithEndDateExactlyOnMilestoneExpectedDate_Succeeds()
        {
            // The boundary: ending exactly on the milestone's due date is allowed, only strictly
            // after it is rejected.
            var fx = new Fixture();
            var projectId = Guid.NewGuid();
            var expectedDate = new DateTime(2026, 1, 15);
            fx.ProjectExists(projectId);
            fx.ProjectService.Setup(s => s.GetProjectByIdAsync(projectId))
                .ReturnsAsync(new MilestoneDTO { MilestoneID = Guid.NewGuid(), ExpectedDate = expectedDate });
            var dto = new SprintCreationDTO
            {
                Name = "Just In Time Sprint",
                Status = SprintStatus.NotStarted,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = expectedDate
            };

            var result = await fx.Repository.CreateSprintAsync(projectId, dto);

            Assert.Equal("Just In Time Sprint", result.Name);
            Assert.Single(fx.Context.Sprints);
        }

        [Fact]
        public async Task CreateSprintAsync_WhenProjectDoesNotExist_ThrowsAndPersistsNothing()
        {
            var fx = new Fixture();
            var projectId = Guid.NewGuid();
            fx.ProjectService.Setup(s => s.CheckProjectExistsAsync(projectId)).ReturnsAsync(ProjectExistence.NotFound);
            var dto = new SprintCreationDTO
            {
                Name = "Ghost Project Sprint",
                Status = SprintStatus.NotStarted,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 1, 15)
            };

            await Assert.ThrowsAsync<ProjectNotFoundException>(() => fx.Repository.CreateSprintAsync(projectId, dto));
            Assert.Empty(fx.Context.Sprints);
            fx.ProjectService.Verify(s => s.GetProjectByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task CreateSprintAsync_WhenProjectExistenceIsUnknown_ThrowsAndPersistsNothing()
        {
            // Fail-closed: Project Service being unreachable/unauthorized is not grounds to let
            // an unverified project through.
            var fx = new Fixture();
            var projectId = Guid.NewGuid();
            fx.ProjectService.Setup(s => s.CheckProjectExistsAsync(projectId)).ReturnsAsync(ProjectExistence.Unknown);
            var dto = new SprintCreationDTO
            {
                Name = "Unverifiable Sprint",
                Status = SprintStatus.NotStarted,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 1, 15)
            };

            await Assert.ThrowsAsync<SprintValidationException>(() => fx.Repository.CreateSprintAsync(projectId, dto));
            Assert.Empty(fx.Context.Sprints);
        }

        // ---------- UpdateSprintAsync ----------

        [Fact]
        public async Task UpdateSprintAsync_ActuallyUpdatesTheTrackedEntity()
        {
            // Reproduces the real bug found during manual testing: PUT used to return 200 and
            // leave every field completely unchanged. This proves the fix (mapping onto the
            // tracked entity via the configured SprintUpdateDTO -> Sprint map) actually applies.
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            fx.Context.Sprints.Add(new Sprint
            {
                Id = id,
                ProjectId = projectId,
                Name = "Original",
                Status = SprintStatus.NotStarted,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 1, 15)
            });
            fx.Context.SaveChanges();
            fx.ProjectExists(projectId);
            fx.ProjectService.Setup(s => s.GetProjectByIdAsync(It.IsAny<Guid>())).ReturnsAsync((MilestoneDTO?)null);

            var update = new SprintUpdateDTO
            {
                ProjectId = projectId,
                Name = "Actually Updated",
                Status = SprintStatus.Active,
                StartDate = new DateTime(2026, 2, 1),
                EndDate = new DateTime(2026, 2, 15)
            };

            var result = await fx.Repository.UpdateSprintAsync(id, update);

            Assert.NotNull(result);
            Assert.Equal("Actually Updated", result!.Name);
            Assert.Equal(SprintStatus.Active, result.Status);

            var persisted = fx.Context.Sprints.Single(s => s.Id == id);
            Assert.Equal("Actually Updated", persisted.Name);
            Assert.Equal(SprintStatus.Active, persisted.Status);
            Assert.Equal(new DateTime(2026, 2, 1), persisted.StartDate);
            Assert.Equal(new DateTime(2026, 2, 15), persisted.EndDate);
        }

        [Fact]
        public async Task UpdateSprintAsync_ForNonexistentId_ReturnsNullWithoutThrowing()
        {
            // Reproduces the real null-dereference risk found during manual testing: this
            // repository method must be safe to call directly (e.g. from here, without going
            // through the controller's separate existence check first).
            var fx = new Fixture();
            var update = new SprintUpdateDTO
            {
                ProjectId = Guid.NewGuid(),
                Name = "Ghost",
                Status = SprintStatus.NotStarted,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 1, 15)
            };

            var result = await fx.Repository.UpdateSprintAsync(Guid.NewGuid(), update);

            Assert.Null(result);
            fx.ProjectService.Verify(s => s.CheckProjectExistsAsync(It.IsAny<Guid>()), Times.Never);
            fx.ProjectService.Verify(s => s.GetProjectByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task UpdateSprintAsync_WhenProjectServiceReturnsNull_StillSucceedsWithoutEnrichment()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            fx.Context.Sprints.Add(new Sprint { Id = id, Name = "Original", ProjectId = Guid.NewGuid() });
            fx.Context.SaveChanges();
            fx.ProjectService.Setup(s => s.CheckProjectExistsAsync(It.IsAny<Guid>())).ReturnsAsync(ProjectExistence.Exists);
            fx.ProjectService.Setup(s => s.GetProjectByIdAsync(It.IsAny<Guid>())).ReturnsAsync((MilestoneDTO?)null);

            var update = new SprintUpdateDTO
            {
                ProjectId = Guid.NewGuid(),
                Name = "Updated Anyway",
                Status = SprintStatus.Active,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 1, 15)
            };

            var result = await fx.Repository.UpdateSprintAsync(id, update);

            Assert.NotNull(result);
            Assert.Equal("Updated Anyway", result!.Name);
            Assert.Null(result.MilestoneId);
        }

        [Fact]
        public async Task UpdateSprintAsync_WithEndDateAfterMilestoneExpectedDate_ThrowsAndLeavesEntityUnchanged()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            fx.Context.Sprints.Add(new Sprint
            {
                Id = id,
                ProjectId = projectId,
                Name = "Original",
                Status = SprintStatus.NotStarted,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 1, 5)
            });
            fx.Context.SaveChanges();
            fx.ProjectExists(projectId);
            fx.ProjectService.Setup(s => s.GetProjectByIdAsync(projectId))
                .ReturnsAsync(new MilestoneDTO { MilestoneID = Guid.NewGuid(), ExpectedDate = new DateTime(2026, 1, 10) });

            var update = new SprintUpdateDTO
            {
                ProjectId = projectId,
                Name = "Pushed Too Far",
                Status = SprintStatus.Active,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 1, 20) // after the milestone's 2026-01-10 due date
            };

            await Assert.ThrowsAsync<SprintValidationException>(() => fx.Repository.UpdateSprintAsync(id, update));

            var persisted = fx.Context.Sprints.Single(s => s.Id == id);
            Assert.Equal("Original", persisted.Name);
            Assert.Equal(new DateTime(2026, 1, 5), persisted.EndDate);
        }

        [Fact]
        public async Task UpdateSprintAsync_WhenProjectDoesNotExist_ThrowsAndLeavesEntityUnchanged()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            var newProjectId = Guid.NewGuid();
            fx.Context.Sprints.Add(new Sprint
            {
                Id = id,
                ProjectId = Guid.NewGuid(),
                Name = "Original",
                Status = SprintStatus.NotStarted,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 1, 5)
            });
            fx.Context.SaveChanges();
            fx.ProjectService.Setup(s => s.CheckProjectExistsAsync(newProjectId)).ReturnsAsync(ProjectExistence.NotFound);

            var update = new SprintUpdateDTO
            {
                ProjectId = newProjectId, // reassigning to a project that doesn't exist
                Name = "Reassigned To Ghost",
                Status = SprintStatus.Active,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 1, 15)
            };

            await Assert.ThrowsAsync<ProjectNotFoundException>(() => fx.Repository.UpdateSprintAsync(id, update));

            var persisted = fx.Context.Sprints.Single(s => s.Id == id);
            Assert.Equal("Original", persisted.Name);
            fx.ProjectService.Verify(s => s.GetProjectByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        // ---------- DeleteSprint ----------

        [Fact]
        public void DeleteSprint_RemovesTheRow()
        {
            var fx = new Fixture();
            var id = Guid.NewGuid();
            fx.Context.Sprints.Add(new Sprint { Id = id, Name = "To delete", ProjectId = Guid.NewGuid() });
            fx.Context.SaveChanges();

            fx.Repository.DeleteSprint(id);

            Assert.Empty(fx.Context.Sprints);
        }

        [Fact]
        public void DeleteSprint_ForNonexistentId_DoesNotThrow()
        {
            var fx = new Fixture();

            var exception = Record.Exception(() => fx.Repository.DeleteSprint(Guid.NewGuid()));

            Assert.Null(exception);
        }
    }
}
