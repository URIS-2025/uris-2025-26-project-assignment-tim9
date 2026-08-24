namespace UserService.Models
{
    public class UserActivityLog
    {
        public Guid LogId { get; set; }
        public Guid UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public Guid PerformedBy { get; set; }
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
