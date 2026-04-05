namespace ProjectService.Models.DTO.ProjectDtos
{
    public class ProjectConfirmationDto
    {
        public Guid ProjectID { get; set; }
        public string Name { get; set; }
        public int Budget { get; set; }
        public string Status { get; set; }
    }
}
