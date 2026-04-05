namespace ProjectService.Models.DTO.RequirementsDtos
{
    public class RequirementsDto
    {
        public Guid RequirementID { get; set; }
        public Guid ProjectID { get; set; }
        public string Description { get; set; }
    }
}
