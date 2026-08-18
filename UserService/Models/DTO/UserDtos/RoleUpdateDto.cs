using System.ComponentModel.DataAnnotations;
using UserService.Models.Enums;
using UserService.Validation;

namespace UserService.Models.DTO.UserDtos
{
    public class RoleUpdateDto
    {
        [NotEmptyGuid]
        public Guid UserId { get; set; }

        [Required]
        public UserRole NewRole { get; set; }

        [NotEmptyGuid]
        public Guid ChangedBy { get; set; }
    }
}
