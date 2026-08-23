namespace WorkPackageService.Models.DTO.DependencyDTOs
{
    public class DependencyDisplayDTO
    {
        public Guid DependencyId { get; set; }
        public Guid TaskId { get; set; }
        public Guid BlockerTaskId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
