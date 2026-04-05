using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProjectService.Models
{
    public class ProjectMember
    {
        public Guid ProjectMemberID { get; set; }
        public Guid ProjectID { get; set; }
        public Date JoinedAt { get; set; }
        public bool Status { get; set; }
    }
}
