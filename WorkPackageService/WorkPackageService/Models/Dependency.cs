namespace WorkPackageService.Models
{
    public class Dependency
    {
        public Guid DependencyId { get; set; }
        public Guid TaskId { get; set; }
        public Guid BlockerTaskId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
