namespace ProjectService.Models.DTO.ProjectDtos
{
    public class ProjectUpdateDto
    {
        public Guid ProjectId { get; set; }
        public string Name { get; set; }
        public int Budget { get; set; }
        public string Status { get; set; }
    }
}
