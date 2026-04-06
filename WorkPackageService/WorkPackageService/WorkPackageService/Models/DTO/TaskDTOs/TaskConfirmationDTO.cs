namespace WorkPackageService.Models.DTO.Task
{
    public class TaskConfirmationDTO
    {
        public Guid TaskId { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public Guid WorkPackageId { get; set; }
    }
}