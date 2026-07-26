namespace WorkPackageService.Models.DTO.DependencyDTOs
{
    public class DependencyCreateDTO
    {
        public Guid TaskId { get; set; }
        public Guid BlockerTaskId { get; set; }
    }
}
