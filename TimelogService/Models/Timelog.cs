namespace TimelogService.Models
{
    public class Timelog
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid WorkPackageId { get; set; }
        public double HoursSpent { get; set; }
        public DateTime Date { get; set; }
    }
}