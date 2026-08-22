namespace SprintService.Data
{
    public class ProjectNotFoundException : SprintValidationException
    {
        public ProjectNotFoundException(Guid projectId)
            : base($"Project {projectId} does not exist.")
        {
        }
    }
}
