using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UserService.Controllers;
using UserService.Data;
using UserService.Models.DTO.AuthDtos;
using UserService.Models.DTO.UserDtos;
using UserService.ServiceCalls.Auth;
using Xunit;

namespace UserService.Tests
{
    public class UserControllerTests
    {
        private readonly Mock<IUserRepository> _repositoryMock = new();
        private readonly Mock<IAuthService> _authServiceMock = new();
        private readonly Mock<IMapper> _mapperMock = new();

        private UserController CreateController()
            => new(_repositoryMock.Object, _authServiceMock.Object, _mapperMock.Object);

        [Fact]
        public void GetUserById_UnknownId_ReturnsNotFound()
        {
            _repositoryMock.Setup(r => r.GetUserById(It.IsAny<Guid>())).Returns((UserDto?)null);
            var controller = CreateController();

            var result = controller.GetUserById(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public void GetUserById_KnownId_ReturnsOkWithUser()
        {
            var dto = new UserDto { UserId = Guid.NewGuid(), Username = "test" };
            _repositoryMock.Setup(r => r.GetUserById(dto.UserId)).Returns(dto);
            var controller = CreateController();

            var result = controller.GetUserById(dto.UserId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(dto, okResult.Value);
        }

        [Fact]
        public void CreateUser_UsernameTaken_ReturnsBadRequest()
        {
            _repositoryMock.Setup(r => r.UsernameExists("taken")).Returns(true);
            var controller = CreateController();

            var result = controller.CreateUser(new UserCreationDto { Username = "taken", Email = "a@a.com" });

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public void ValidateCredentials_InvalidCredentials_ReturnsUnauthorized()
        {
            _repositoryMock
                .Setup(r => r.ValidateCredentials("bob", "wrong"))
                .Returns(new UserAuthVo { IsValid = false });
            var controller = CreateController();

            var result = controller.ValidateCredentials(new CredentialsValidationDto { Username = "bob", Password = "wrong" });

            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        public async Task ChangeRole_RepositoryRejects_ReturnsBadRequest()
        {
            _repositoryMock.Setup(r => r.ChangeRole(It.IsAny<RoleUpdateDto>())).ReturnsAsync(false);
            var controller = CreateController();

            var result = await controller.ChangeRole(new RoleUpdateDto { UserId = Guid.NewGuid(), ChangedBy = Guid.NewGuid() });

            Assert.IsType<BadRequestObjectResult>(result);
            _authServiceMock.Verify(a => a.RevokeSessionsAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task ChangeRole_Success_RevokesSessions()
        {
            _repositoryMock.Setup(r => r.ChangeRole(It.IsAny<RoleUpdateDto>())).ReturnsAsync(true);
            var controller = CreateController();
            var userId = Guid.NewGuid();

            var result = await controller.ChangeRole(new RoleUpdateDto { UserId = userId, ChangedBy = Guid.NewGuid() });

            Assert.IsType<NoContentResult>(result);
            _authServiceMock.Verify(a => a.RevokeSessionsAsync(userId), Times.Once);
        }

        [Fact]
        public async Task DeactivateUser_UnknownUser_ReturnsNotFound()
        {
            _repositoryMock.Setup(r => r.SetActiveStatus(It.IsAny<Guid>(), false, It.IsAny<Guid>())).Returns(false);
            var controller = CreateController();

            var result = await controller.DeactivateUser(Guid.NewGuid(), Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
