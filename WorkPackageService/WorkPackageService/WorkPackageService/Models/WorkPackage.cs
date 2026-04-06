using System.ComponentModel.DataAnnotations.Schema;
using WorkPackageService.Models.Enums;

namespace WorkPackageService.Models
{
    public class WorkPackage
    {
        public Guid WorkPackageId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public WorkPackageStatus Status { get; set; }
        public int Budget { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid ProjectId { get; set; }
        public ICollection<Task> Tasks { get; set; }
        public Backlog Backlog { get; set; }
    }
}