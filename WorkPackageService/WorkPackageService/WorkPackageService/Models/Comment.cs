namespace WorkPackageService.Models
{
    public class Comment
    {
        public Guid CommentId { get; set; }
        public string Text { get; set; }
        public string Username { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid TaskId { get; set; }
        public Task Task { get; set; }
    }
}