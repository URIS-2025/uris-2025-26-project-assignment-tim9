namespace WorkPackageService.Models.DTO.BacklogDTOs
{
    public class BacklogCreateDTO
    {
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
