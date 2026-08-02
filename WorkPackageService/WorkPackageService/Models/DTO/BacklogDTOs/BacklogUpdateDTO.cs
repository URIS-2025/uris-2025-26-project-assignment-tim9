using System.ComponentModel.DataAnnotations;
using WorkPackageService.Validation;

namespace WorkPackageService.Models.DTO.BacklogDTOs
{
    public class BacklogUpdateDTO
    {
        [NotEmptyGuid]
        public Guid Id { get; set; }

        [StringLength(200)]
        public string? Name { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }
    }
}
