namespace WorkPackageService.Models.DTO.BacklogDTOs
{
    public class BacklogDisplayDTO
    {
        public Guid BacklogId { get; set; }
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
