namespace TimelogService.Models.DTO
{
    public class TimelogDTO
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid TaskId { get; set; }
        public double HoursSpent { get; set; }
        public DateTime Date { get; set; }
        public Guid LoggedByUserId { get; set; }
    }
}