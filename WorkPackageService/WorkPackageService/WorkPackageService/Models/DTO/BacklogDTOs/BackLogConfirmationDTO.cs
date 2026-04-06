namespace WorkPackageService.Models.DTO.BackLog
{
    public class BackLogConfirmationDTO
    {
        public Guid BackLogId { get; set; }
        public string Name { get; set; }
        public Guid WorkPackageId { get; set; }
    }
}