namespace ProjectService.Models.DTO.RequirementsDtos
{
    public class RequirementsUpdateDto
    {
        public Guid RequirementId { get; set; }
        public Guid ProjectId { get; set; }
        public string Description { get; set; }
    }
}
