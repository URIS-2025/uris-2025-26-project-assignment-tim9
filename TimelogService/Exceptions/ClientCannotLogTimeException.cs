namespace TimelogService.Exceptions
{
    public class ClientCannotLogTimeException : Exception
    {
        public Guid UserId { get; }

        public ClientCannotLogTimeException(Guid userId)
            : base($"User '{userId}' has the Client role and cannot log time.")
        {
            UserId = userId;
        }
    }
}
