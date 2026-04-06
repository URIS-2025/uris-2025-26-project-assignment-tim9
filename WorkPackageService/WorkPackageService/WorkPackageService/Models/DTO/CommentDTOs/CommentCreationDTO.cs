namespace WorkPackageService.Models.DTO.Comment
{
    public class CommentCreationDTO
    {
        public string Text { get; set; }
        public string Username { get; set; }
        public Guid TaskId { get; set; }
    }
}