namespace WorkPackageService.Models.DTO.Comment
{
    public class CommentConfirmationDTO
    {
        public Guid CommentId { get; set; }
        public string Text { get; set; }
        public string Username { get; set; }
        public Guid TaskId { get; set; }
    }
}