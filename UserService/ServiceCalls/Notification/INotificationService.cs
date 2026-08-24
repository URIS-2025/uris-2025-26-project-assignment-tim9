namespace UserService.ServiceCalls.Notification
{
    public interface INotificationService
    {
        Task<bool> SendNotificationAsync(Guid userId, string message, string type);
    }
}
