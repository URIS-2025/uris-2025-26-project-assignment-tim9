namespace WorkPackageService.Models.DTO.BackLog
{
    public class BackLogCreationDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid CreatedByProjectMember { get; set; }
        public Guid WorkPackageId { get; set; }
    }
}