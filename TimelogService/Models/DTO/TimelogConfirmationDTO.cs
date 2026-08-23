
namespace TimelogService.Models.DTO
{
    public class TimelogConfirmationDTO
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;
        public string TaskStatus { get; set; } = string.Empty;
        public double HoursSpent { get; set; }
        public DateTime Date { get; set; }
    }
}
