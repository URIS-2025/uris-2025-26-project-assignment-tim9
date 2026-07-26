namespace WorkPackageService.Models.DTO.DependencyDTOs
{
    public class DependencyUpdateDTO
    {
        // No NotEqualToProperty(TaskId) here - this DTO has no TaskId field to compare
        // against (the owning task isn't editable). Self-block on update is still
        // enforced in DependencyRepository.Update against the entity's existing TaskId.
        public Guid? BlockerTaskId { get; set; }
    }
}
