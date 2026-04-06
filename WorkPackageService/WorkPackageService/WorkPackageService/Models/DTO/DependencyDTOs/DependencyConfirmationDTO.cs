namespace WorkPackageService.Models.DTO.Dependency
{
    public class DependencyConfirmationDTO
    {
        public Guid DependencyId { get; set; }
        public Guid TaskId { get; set; }
        public Guid BlockerTaskId { get; set; }
    }
}