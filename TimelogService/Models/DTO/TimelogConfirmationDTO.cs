
namespace TimelogService.Models.DTO
{
    public class TimelogConfirmationDTO
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string WorkPackageTitle { get; set; } = string.Empty;
        public double HoursSpent { get; set; }
        public DateTime Date { get; set; }
    }
}