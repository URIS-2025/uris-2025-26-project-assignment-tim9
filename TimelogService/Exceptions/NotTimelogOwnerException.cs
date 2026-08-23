namespace TimelogService.Exceptions
{
    public class NotTimelogOwnerException : Exception
    {
        public Guid ActingUserId { get; }
        public Guid OwnerUserId { get; }

        public NotTimelogOwnerException(Guid actingUserId, Guid ownerUserId)
            : base($"User '{actingUserId}' can only update or delete their own timelogs.")
        {
            ActingUserId = actingUserId;
            OwnerUserId = ownerUserId;
        }
    }
}
