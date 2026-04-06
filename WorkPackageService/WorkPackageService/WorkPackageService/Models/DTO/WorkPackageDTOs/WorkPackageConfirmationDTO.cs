namespace WorkPackageService.Models.DTO.WorkPackage
{
    public class WorkPackageConfirmationDTO
    {
        public Guid WorkPackageId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public Guid ProjectId { get; set; }
    }
}