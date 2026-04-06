using System.ComponentModel.DataAnnotations;

namespace TimelogService.Models.DTO
{
    public class TimelogCreationDTO
    {
        [Required(ErrorMessage = "Project ID is required.")]
        public Guid ProjectId { get; set; }

        [Required(ErrorMessage = "WorkPackage ID is required.")]
        public Guid WorkPackageId { get; set; }

        [Required(ErrorMessage = "Hours spent is required.")]
        public double HoursSpent { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        public DateTime Date { get; set; }
    }
}