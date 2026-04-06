using TimelogService.Validation;

namespace TimelogService.Models.DTO
{
    public class TimelogUpdateDTO
    {
        public Guid? ProjectId { get; set; }
        public Guid? WorkPackageId { get; set; }
        public double? HoursSpent { get; set; }
        [NoFutureDate(ErrorMessage = "Timelog date can not be in the future time.")]
        public DateTime? Date { get; set; }
    }
}