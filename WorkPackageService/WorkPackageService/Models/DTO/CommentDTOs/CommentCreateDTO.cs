using System.ComponentModel.DataAnnotations;
using WorkPackageService.Validation;

namespace WorkPackageService.Models.DTO.CommentDTOs
{
    public class CommentCreateDTO
    {
        [NotEmptyGuid]
        public Guid TaskId { get; set; }

        [NotEmptyGuid]
        public Guid AuthorId { get; set; }

        [Required]
        [StringLength(2000)]
        public string Text { get; set; } = string.Empty;
    }
}
