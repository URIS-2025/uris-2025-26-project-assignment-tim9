namespace TimelogService.Models.DTO
{
    public class TimelogUpdateDTO
    {
        public Guid? ProjectId { get; set; }
        public Guid? WorkPackageId { get; set; }
        public double? HoursSpent { get; set; }
        public DateTime? Date { get; set; }
    }
}