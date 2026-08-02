using System.ComponentModel.DataAnnotations;
using WorkPackageService.Validation;

namespace WorkPackageService.Models.DTO.CommentDTOs
{
    public class CommentUpdateDTO
    {
        [NotEmptyGuid]
        public Guid Id { get; set; }

        [StringLength(2000)]
        public string? Text { get; set; }
    }
}
