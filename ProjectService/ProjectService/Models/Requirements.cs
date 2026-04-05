namespace ProjectService.Models
{
    public class Requirements
    {
        public Guid RequirementID { get; set; }
        public Guid ProjectID { get; set; }
        public string Description { get; set; }
    }
}
