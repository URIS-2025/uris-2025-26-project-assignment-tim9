using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using AuthService.Controllers;
using AuthService.Data;
using AuthService.Models;
using AuthService.Models.DTO.AuthDtos;
using AuthService.Models.DTO.UserDtos;
using AuthService.Models.Enums;
using AuthService.ServiceCalls.User;
using AuthService.Services;
using Xunit;

namespace AuthService.Tests
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthRepository> _repositoryMock = new();
        private readonly Mock<IUserService> _userServiceMock = new();
        private readonly Mock<ITokenService> _tokenServiceMock = new();
        private readonly IConfiguration _configuration = new ConfigurationBuilder().Build();

        private AuthController CreateController()
            => new(_repositoryMock.Object, _userServiceMock.Object, _tokenServiceMock.Object, _configuration);

        [Fact]
        public async Task Login_UserServiceUnavailable_Returns503()
        {
            _userServiceMock
                .Setup(u => u.ValidateCredentialsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((UserAuthVo?)null);
            var controller = CreateController();

            var result = await controller.Login(new LoginDto { Username = "admin", Password = "x" });

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(503, objectResult.StatusCode);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            _userServiceMock
                .Setup(u => u.ValidateCredentialsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new UserAuthVo { IsValid = false });
            var controller = CreateController();

            var result = await controller.Login(new LoginDto { Username = "admin", Password = "wrong" });

            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        public async Task Login_DeactivatedUser_ReturnsUnauthorized()
        {
            _userServiceMock
                .Setup(u => u.ValidateCredentialsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new UserAuthVo { IsValid = true, IsActive = false, Username = "admin", Role = "Admin" });
            var controller = CreateController();

            var result = await controller.Login(new LoginDto { Username = "admin", Password = "x" });

            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        public async Task Login_ValidActiveUser_CreatesSessionAndReturnsToken()
        {
            var userId = Guid.NewGuid();
            _userServiceMock
                .Setup(u => u.ValidateCredentialsAsync("admin", "correct"))
                .ReturnsAsync(new UserAuthVo { IsValid = true, IsActive = true, UserId = userId, Username = "admin", Role = "Admin" });
            _tokenServiceMock
                .Setup(t => t.GenerateAccessToken(userId, "admin", "Admin"))
                .Returns(("jwt-token", DateTime.UtcNow.AddMinutes(15)));
            _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("refresh-token-xyz");
            var controller = CreateController();

            var result = await controller.Login(new LoginDto { Username = "admin", Password = "correct" });

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<TokenResponseDto>(okResult.Value);
            Assert.Equal("jwt-token", response.AccessToken);
            Assert.Equal("refresh-token-xyz", response.RefreshToken);
            _repositoryMock.Verify(r => r.CreateSession(userId, "admin", "Admin", "refresh-token-xyz", It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public void Refresh_UnknownToken_ReturnsUnauthorized()
        {
            _repositoryMock.Setup(r => r.GetByRefreshToken(It.IsAny<string>())).Returns((AuthSession?)null);
            var controller = CreateController();

            var result = controller.Refresh(new RefreshDto { RefreshToken = "nope" });

            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        public void Refresh_RevokedSession_ReturnsUnauthorized()
        {
            _repositoryMock.Setup(r => r.GetByRefreshToken("revoked-token")).Returns(new AuthSession
            {
                AuthId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Username = "admin",
                Permission = UserRole.Admin,
                Token = "revoked-token",
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                IsRevoked = true
            });
            var controller = CreateController();

            var result = controller.Refresh(new RefreshDto { RefreshToken = "revoked-token" });

            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        public void Refresh_ExpiredSession_ReturnsUnauthorized()
        {
            _repositoryMock.Setup(r => r.GetByRefreshToken("expired-token")).Returns(new AuthSession
            {
                AuthId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Username = "admin",
                Permission = UserRole.Admin,
                Token = "expired-token",
                ExpiresAt = DateTime.UtcNow.AddDays(-1),
                IsRevoked = false
            });
            var controller = CreateController();

            var result = controller.Refresh(new RefreshDto { RefreshToken = "expired-token" });

            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        public void Logout_UnknownToken_ReturnsNotFound()
        {
            _repositoryMock.Setup(r => r.RevokeSession(It.IsAny<string>())).Returns(false);
            var controller = CreateController();

            var result = controller.Logout(new RefreshDto { RefreshToken = "nope" });

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Logout_ValidToken_ReturnsNoContent()
        {
            _repositoryMock.Setup(r => r.RevokeSession("valid-token")).Returns(true);
            var controller = CreateController();

            var result = controller.Logout(new RefreshDto { RefreshToken = "valid-token" });

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void RevokeAllSessions_AlwaysReturnsNoContent()
        {
            var controller = CreateController();

            var result = controller.RevokeAllSessions(Guid.NewGuid());

            Assert.IsType<NoContentResult>(result);
            _repositoryMock.Verify(r => r.RevokeAllSessionsForUser(It.IsAny<Guid>()), Times.Once);
        }
    }
}
