namespace WorkPackageService.Models.DTO.WorkPackage
{
    public class WorkPackageDTO
    {
        public Guid WorkPackageId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int Budget { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid ProjectId { get; set; }
    }
}