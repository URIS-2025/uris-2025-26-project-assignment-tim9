using System.ComponentModel.DataAnnotations;

namespace WorkPackageService.Models.DTO.CommentDTOs
{
    public class CommentUpdateDTO
    {
        [StringLength(2000)]
        public string? Text { get; set; }
    }
}
