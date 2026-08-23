using System;
using System.Threading.Tasks;
using ProjectService.Models.DTO.UserDtos;
using ProjectService.ServiceCalls.User;

namespace ProjectService.Tests.Integration
{
    public class FakeUserService : IUserService
    {
        public static readonly Guid UnknownUserId = Guid.Parse("00000000-0000-0000-0000-000000000404");

        public const string TestUsername = "Test User";
        public const string TestRole = "TeamMember";

        public Task<UserProjectDto> GetUserByIdAsync(Guid userId)
        {
            if (userId == UnknownUserId)
                return Task.FromResult<UserProjectDto>(null!);

            return Task.FromResult(new UserProjectDto
            {
                Username = TestUsername,
                Role = TestRole
            });
        }
    }
}
