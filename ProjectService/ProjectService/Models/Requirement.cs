using System.ComponentModel.DataAnnotations;

namespace ProjectService.Models
{
    public class Requirement
    {
        
        public Guid RequirementId { get; set; }
        public Guid ProjectId { get; set; }
        public string Description { get; set; }
    }
}
