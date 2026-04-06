namespace WorkPackageService.Models.DTO.Dependency
{
    public class DependencyCreationDTO
    {
        public Guid TaskId { get; set; }
        public Guid BlockerTaskId { get; set; }
    }
}