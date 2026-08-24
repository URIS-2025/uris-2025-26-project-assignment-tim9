namespace PaymentService.Models.DTO.Project
{
    //podskup ProjectDto iz Project servisa
    public class ProjectInfoDTO
    {
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Budget { get; set; }
    }
}
