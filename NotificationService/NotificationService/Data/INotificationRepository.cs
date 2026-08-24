using NotificationService.Models;

namespace NotificationService.Data
{
    public interface INotificationRepository
    {
        Task<Notification> CreateAsync(Notification notification);

        Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);

        Task<Notification?> GetByIdAsync(Guid id);

        Task<Notification> MarkAsReadAsync(Guid id);
    }
}
