using AutoMapper;
using AttachmentService.Models;
using AttachmentService.Models.DTO;
using AttachmentService.Models.Enums;
using AttachmentService.Profiles;
using Microsoft.Extensions.Logging.Abstractions;

namespace AttachmentService.Tests
{
    public class AttachmentProfileTests
    {
        private static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<AttachmentProfile>(), NullLoggerFactory.Instance);
            return config.CreateMapper();
        }

        [Fact]
        public void Configuration_IsValid()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<AttachmentProfile>(), NullLoggerFactory.Instance);

            // Throws if any CreateMap in the profile is missing a required member mapping.
            config.AssertConfigurationIsValid();
        }

        [Fact]
        public void Map_CreationDtoToAttachment_CopiesSuppliedFields()
        {
            var mapper = CreateMapper();
            var dto = new AttachmentCreationDTO
            {
                OriginalFileName = "spec.pdf",
                ContentType = "application/pdf",
                FileSize = 500,
                ProjectId = Guid.NewGuid(),
                WorkPackageId = Guid.NewGuid()
            };

            var entity = mapper.Map<Attachment>(dto);

            Assert.Equal(dto.OriginalFileName, entity.OriginalFileName);
            Assert.Equal(dto.ContentType, entity.ContentType);
            Assert.Equal(dto.FileSize, entity.FileSize);
            Assert.Equal(dto.ProjectId, entity.ProjectId);
            Assert.Equal(dto.WorkPackageId, entity.WorkPackageId);
        }

        [Fact]
        public void Map_AttachmentToDto_CopiesAllLocalFields()
        {
            var mapper = CreateMapper();
            var entity = new Attachment
            {
                Id = Guid.NewGuid(),
                FileName = "abc_spec.pdf",
                OriginalFileName = "spec.pdf",
                StoragePath = "projects/x/abc_spec.pdf",
                ContentType = "application/pdf",
                FileSize = 500,
                Checksum = "deadbeef",
                CreatedAt = DateTime.UtcNow,
                Description = "desc",
                Status = AttachmentStatus.Ready,
                ProjectId = Guid.NewGuid(),
                WorkPackageId = Guid.NewGuid(),
                UploadedByUserId = Guid.NewGuid()
            };

            var dto = mapper.Map<AttachmentDTO>(entity);

            Assert.Equal(entity.Id, dto.Id);
            Assert.Equal(entity.FileName, dto.FileName);
            Assert.Equal(entity.OriginalFileName, dto.OriginalFileName);
            Assert.Equal(entity.ContentType, dto.ContentType);
            Assert.Equal(entity.FileSize, dto.FileSize);
            Assert.Equal(entity.Checksum, dto.Checksum);
            Assert.Equal(entity.Description, dto.Description);
            Assert.Equal(entity.Status, dto.Status);
            Assert.Equal(entity.ProjectId, dto.ProjectId);
            Assert.Equal(entity.WorkPackageId, dto.WorkPackageId);
            Assert.Equal(entity.UploadedByUserId, dto.UploadedByUserId);
        }

        [Fact]
        public void Map_UpdateDtoOntoEntity_OnlyOverwritesSuppliedFields()
        {
            var mapper = CreateMapper();
            var entity = new Attachment
            {
                FileName = "original-name.txt",
                Description = "original description"
            };

            // Only Description is being changed - FileName is deliberately omitted (null).
            var update = new AttachmentUpdateDTO { FileName = null, Description = "new description" };

            mapper.Map(update, entity);

            Assert.Equal("original-name.txt", entity.FileName);
            Assert.Equal("new description", entity.Description);
        }

        [Fact]
        public void Map_UpdateDtoWithBothFields_OverwritesBoth()
        {
            var mapper = CreateMapper();
            var entity = new Attachment
            {
                FileName = "original-name.txt",
                Description = "original description"
            };

            var update = new AttachmentUpdateDTO { FileName = "renamed.txt", Description = "new description" };

            mapper.Map(update, entity);

            Assert.Equal("renamed.txt", entity.FileName);
            Assert.Equal("new description", entity.Description);
        }
    }
}
