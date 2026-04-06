namespace WorkPackageService.Models.DTO.Dependency
{
    public class DependencyDTO
    {
        public Guid DependencyId { get; set; }
        public Guid TaskId { get; set; }
        public Guid BlockerTaskId { get; set; }
    }
}