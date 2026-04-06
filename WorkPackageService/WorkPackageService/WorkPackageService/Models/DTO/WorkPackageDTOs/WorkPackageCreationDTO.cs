namespace WorkPackageService.Models.DTO.WorkPackage
{
    public class WorkPackageCreationDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int Budget { get; set; }
        public Guid ProjectId { get; set; }
    }
}