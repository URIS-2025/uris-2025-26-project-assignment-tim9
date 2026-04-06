namespace WorkPackageService.Models
{
    public class BackLog
    {
        public Guid BackLogId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid CreatedByProjectMember { get; set; }
        public Guid WorkPackageId { get; set; }
        public WorkPackage WorkPackage { get; set; }
    }
}