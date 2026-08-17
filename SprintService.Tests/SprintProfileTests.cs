using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using SprintService.Models;
using SprintService.Models.DTO;
using SprintService.Models.Enums;
using SprintService.Profiles;

namespace SprintService.Tests
{
    public class SprintProfileTests
    {
        private static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<SprintProfile>(), NullLoggerFactory.Instance);
            return config.CreateMapper();
        }

        [Fact]
        public void Configuration_IsValid()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<SprintProfile>(), NullLoggerFactory.Instance);

            config.AssertConfigurationIsValid();
        }

        [Fact]
        public void Map_CreationDtoToSprint_CopiesSuppliedFields()
        {
            var mapper = CreateMapper();
            var dto = new SprintCreationDTO
            {
                Name = "Sprint Alpha",
                Status = SprintStatus.Active,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 1, 15)
            };

            var entity = mapper.Map<Sprint>(dto);

            // ProjectId isn't on SprintCreationDTO - the repository sets it from the route.
            Assert.Equal(Guid.Empty, entity.ProjectId);
            Assert.Equal(dto.Name, entity.Name);
            Assert.Equal(dto.Status, entity.Status);
            Assert.Equal(dto.StartDate, entity.StartDate);
            Assert.Equal(dto.EndDate, entity.EndDate);
        }

        [Fact]
        public void Map_SprintToDto_CopiesAllFields()
        {
            var mapper = CreateMapper();
            var entity = new Sprint
            {
                Id = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                Name = "Sprint Alpha",
                Status = SprintStatus.Completed,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 1, 15)
            };

            var dto = mapper.Map<SprintDTO>(entity);

            Assert.Equal(entity.Id, dto.Id);
            Assert.Equal(entity.ProjectId, dto.ProjectId);
            Assert.Equal(entity.Name, dto.Name);
            Assert.Equal(entity.Status, dto.Status);
            Assert.Equal(entity.StartDate, dto.StartDate);
            Assert.Equal(entity.EndDate, dto.EndDate);
        }

        [Fact]
        public void Map_UpdateDtoOntoEntity_ReplacesAllFields()
        {
            var mapper = CreateMapper();
            var originalId = Guid.NewGuid();
            var entity = new Sprint
            {
                Id = originalId,
                ProjectId = Guid.NewGuid(),
                Name = "Old Name",
                Status = SprintStatus.NotStarted,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 1, 15)
            };
            var update = new SprintUpdateDTO
            {
                ProjectId = entity.ProjectId,
                Name = "New Name",
                Status = SprintStatus.Active,
                StartDate = new DateTime(2026, 2, 1),
                EndDate = new DateTime(2026, 2, 15)
            };

            mapper.Map(update, entity);

            Assert.Equal("New Name", entity.Name);
            Assert.Equal(SprintStatus.Active, entity.Status);
            Assert.Equal(new DateTime(2026, 2, 1), entity.StartDate);
            Assert.Equal(new DateTime(2026, 2, 15), entity.EndDate);
            // Id isn't on SprintUpdateDTO - the map must leave the tracked entity's Id alone.
            Assert.Equal(originalId, entity.Id);
        }

        [Fact]
        public void Map_SprintToConfirmationDto_DoesNotTouchMilestoneFields()
        {
            var mapper = CreateMapper();
            var entity = new Sprint
            {
                Id = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                Name = "Sprint Alpha",
                Status = SprintStatus.Active,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 1, 15)
            };

            var confirmation = mapper.Map<SprintConfirmationDTO>(entity);

            Assert.Equal(entity.Id, confirmation.Id);
            Assert.Equal(entity.Name, confirmation.Name);
            // Milestone fields aren't on Sprint at all - the repository fills these in
            // separately from Project Service, the mapper must leave them alone.
            Assert.Null(confirmation.MilestoneId);
            Assert.Null(confirmation.ExpectedDate);
        }
    }
}
