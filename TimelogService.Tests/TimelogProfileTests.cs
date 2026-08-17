using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using TimelogService.Models;
using TimelogService.Models.DTO;
using TimelogService.Profiles;

namespace TimelogService.Tests
{
    public class TimelogProfileTests
    {
        private static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<TimelogProfile>(), NullLoggerFactory.Instance);
            return config.CreateMapper();
        }

        [Fact]
        public void Configuration_IsValid()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<TimelogProfile>(), NullLoggerFactory.Instance);

            config.AssertConfigurationIsValid();
        }

        [Fact]
        public void Map_CreationDtoToTimelog_CopiesSuppliedFields()
        {
            var mapper = CreateMapper();
            var dto = new TimelogCreationDTO
            {
                ProjectId = Guid.NewGuid(),
                WorkPackageId = Guid.NewGuid(),
                HoursSpent = 3.5,
                Date = new DateTime(2026, 1, 1)
            };

            var entity = mapper.Map<Timelog>(dto);

            Assert.Equal(dto.ProjectId, entity.ProjectId);
            Assert.Equal(dto.WorkPackageId, entity.WorkPackageId);
            Assert.Equal(dto.HoursSpent, entity.HoursSpent);
            Assert.Equal(dto.Date, entity.Date);
            Assert.Equal(Guid.Empty, entity.LoggedByUserId); // repository sets this, not the mapper
        }

        [Fact]
        public void Map_TimelogToDto_CopiesAllFields()
        {
            var mapper = CreateMapper();
            var entity = new Timelog
            {
                Id = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                WorkPackageId = Guid.NewGuid(),
                HoursSpent = 2.25,
                Date = new DateTime(2026, 1, 1),
                LoggedByUserId = Guid.NewGuid()
            };

            var dto = mapper.Map<TimelogDTO>(entity);

            Assert.Equal(entity.Id, dto.Id);
            Assert.Equal(entity.ProjectId, dto.ProjectId);
            Assert.Equal(entity.WorkPackageId, dto.WorkPackageId);
            Assert.Equal(entity.HoursSpent, dto.HoursSpent);
            Assert.Equal(entity.Date, dto.Date);
            Assert.Equal(entity.LoggedByUserId, dto.LoggedByUserId);
        }

        // ---------- The bug found during manual testing: nullable value types + ForAllMembers ----------

        [Fact]
        public void Map_UpdateDtoWithOnlyHoursSpent_LeavesOtherFieldsUntouched()
        {
            // This is the exact scenario that was broken: ForAllMembers(Condition(...)) does
            // not reliably skip a null Nullable<T> (Guid?, double?, DateTime?) source when the
            // destination is a non-nullable value type - it silently overwrote ProjectId/
            // WorkPackageId/Date with their zero-defaults instead of leaving them alone. Fixed
            // with per-member .ForMember(...).Condition(...) instead.
            var mapper = CreateMapper();
            var originalProjectId = Guid.NewGuid();
            var originalWorkPackageId = Guid.NewGuid();
            var originalDate = new DateTime(2026, 1, 1);
            var entity = new Timelog
            {
                Id = Guid.NewGuid(),
                ProjectId = originalProjectId,
                WorkPackageId = originalWorkPackageId,
                HoursSpent = 3.0,
                Date = originalDate,
                LoggedByUserId = Guid.NewGuid()
            };
            var update = new TimelogUpdateDTO { HoursSpent = 7.25 };

            mapper.Map(update, entity);

            Assert.Equal(7.25, entity.HoursSpent);
            Assert.Equal(originalProjectId, entity.ProjectId);
            Assert.Equal(originalWorkPackageId, entity.WorkPackageId);
            Assert.Equal(originalDate, entity.Date);
        }

        [Fact]
        public void Map_UpdateDtoWithAllFieldsNull_ChangesNothing()
        {
            var mapper = CreateMapper();
            var entity = new Timelog
            {
                Id = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                WorkPackageId = Guid.NewGuid(),
                HoursSpent = 3.0,
                Date = new DateTime(2026, 1, 1),
                LoggedByUserId = Guid.NewGuid()
            };
            var originalProjectId = entity.ProjectId;
            var originalWorkPackageId = entity.WorkPackageId;
            var originalHours = entity.HoursSpent;
            var originalDate = entity.Date;

            mapper.Map(new TimelogUpdateDTO(), entity);

            Assert.Equal(originalProjectId, entity.ProjectId);
            Assert.Equal(originalWorkPackageId, entity.WorkPackageId);
            Assert.Equal(originalHours, entity.HoursSpent);
            Assert.Equal(originalDate, entity.Date);
        }

        [Fact]
        public void Map_UpdateDtoWithAllFieldsSet_ReplacesAll()
        {
            var mapper = CreateMapper();
            var entity = new Timelog
            {
                Id = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                WorkPackageId = Guid.NewGuid(),
                HoursSpent = 3.0,
                Date = new DateTime(2026, 1, 1),
                LoggedByUserId = Guid.NewGuid()
            };
            var newProjectId = Guid.NewGuid();
            var newWorkPackageId = Guid.NewGuid();
            var newDate = new DateTime(2026, 3, 1);
            var update = new TimelogUpdateDTO
            {
                ProjectId = newProjectId,
                WorkPackageId = newWorkPackageId,
                HoursSpent = 9.5,
                Date = newDate
            };

            mapper.Map(update, entity);

            Assert.Equal(newProjectId, entity.ProjectId);
            Assert.Equal(newWorkPackageId, entity.WorkPackageId);
            Assert.Equal(9.5, entity.HoursSpent);
            Assert.Equal(newDate, entity.Date);
        }

        [Fact]
        public void Map_TimelogToConfirmationDto_DoesNotTouchEnrichmentFields()
        {
            var mapper = CreateMapper();
            var entity = new Timelog
            {
                Id = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                WorkPackageId = Guid.NewGuid(),
                HoursSpent = 4,
                Date = new DateTime(2026, 1, 1),
                LoggedByUserId = Guid.NewGuid()
            };

            var confirmation = mapper.Map<TimelogConfirmationDTO>(entity);

            Assert.Equal(entity.Id, confirmation.Id);
            Assert.Equal(entity.HoursSpent, confirmation.HoursSpent);
            // Username/WorkPackageTitle come from Project Service/WorkPackage Service, not
            // from Timelog - the mapper must leave them at their default.
            Assert.Equal(string.Empty, confirmation.Username);
            Assert.Equal(string.Empty, confirmation.WorkPackageTitle);
        }
    }
}
