using System.ComponentModel.DataAnnotations;

namespace WorkPackageService.Models.DTO.BacklogDTOs
{
    public class BacklogUpdateDTO
    {
        [StringLength(200)]
        public string? Name { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }
    }
}
