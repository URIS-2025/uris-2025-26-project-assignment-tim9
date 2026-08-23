namespace WorkPackageService.Models.DTO.CommentDTOs
{
    public class CommentDisplayDTO
    {
        public Guid CommentId { get; set; }
        public Guid TaskId { get; set; }
        public Guid AuthorId { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
