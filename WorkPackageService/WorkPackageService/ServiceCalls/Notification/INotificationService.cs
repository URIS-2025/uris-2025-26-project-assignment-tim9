namespace WorkPackageService.ServiceCalls.Notification
{
    public interface INotificationService
    {
        // Task<bool> ovde je System.Threading.Tasks.Task<bool> (async rezultat), ne domenski
        // Task entitet - ovaj fajl ne importuje WorkPackageService.Models, pa nema kolizije.
        Task<bool> SendNotificationAsync(Guid userId, string message, string type);
    }
}
