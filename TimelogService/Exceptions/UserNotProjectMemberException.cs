namespace TimelogService.Exceptions
{
    public class UserNotProjectMemberException : Exception
    {
        public Guid UserId { get; }
        public Guid ProjectId { get; }

        public UserNotProjectMemberException(Guid userId, Guid projectId)
            : base($"User '{userId}' is not an active member of project '{projectId}'.")
        {
            UserId = userId;
            ProjectId = projectId;
        }
    }
}
