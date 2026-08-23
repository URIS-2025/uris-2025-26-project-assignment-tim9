using Microsoft.EntityFrameworkCore;
using NotificationService.Context;
using NotificationService.Exceptions;
using NotificationService.Models;

namespace NotificationService.Data
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationServiceContext _context;

        public NotificationRepository(NotificationServiceContext context)
        {
            _context = context;
        }

        public async Task<Notification> CreateAsync(Notification notification)
        {
            notification.Id = Guid.NewGuid();
            notification.CreatedAt = DateTime.UtcNow;
            notification.IsRead = false;

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return notification;
        }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            return await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<Notification> MarkAsReadAsync(Guid id)
        {
            var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id)
                ?? throw new EntityNotFoundException($"Notifikacija sa ID-jem {id} ne postoji.");

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return notification;
        }
    }
}
