using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProjectService.Models
{
    public class ProjectMember
    {
        [Key]
        public Guid ProjectMemberId { get; set; }
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool Status { get; set; }
    }
}
