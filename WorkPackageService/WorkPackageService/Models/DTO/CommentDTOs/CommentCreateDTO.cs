namespace WorkPackageService.Models.DTO.CommentDTOs
{
    public class CommentCreateDTO
    {
        public Guid TaskId { get; set; }
        public Guid AuthorId { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
