namespace AttachmentService.Exceptions
{
    public class TaskNotFoundException : Exception
    {
        public Guid TaskId { get; }

        public TaskNotFoundException(Guid taskId)
            : base($"Task '{taskId}' does not exist.")
        {
            TaskId = taskId;
        }
    }
}
