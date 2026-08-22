using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models.DTO.NotificationDTOs
{
    // Ugovor je fiksiran od strane WorkPackageService.ServiceCalls.Notification.NotificationService,
    // koji vec salje POST /notifications sa telom { userId, message, type }. Imena polja ovde
    // moraju da ostanu userId/message/type da se ne bi pokvario vec napisan pozivajuci kod.
    public class NotificationCreateDTO
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Type { get; set; } = string.Empty;
    }
}
