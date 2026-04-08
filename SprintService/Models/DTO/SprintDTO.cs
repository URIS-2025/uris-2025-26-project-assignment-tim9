using SprintService.Models.Enums;

namespace SprintService.Models.DTO
{
    public class SprintDTO
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Name { get; set; }
        public SprintStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
