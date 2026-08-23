using IntegrationService.Models;

namespace IntegrationService.Data
{
    public interface IIntegrationRepository
    {
        Task<Integration> CreateAsync(Integration integration);

        Task<IEnumerable<Integration>> GetAllAsync();

        Task<Integration?> GetByIdAsync(Guid id);

        Task<Integration> UpdateAsync(Guid id, Integration updated, bool rotateApiKey);

        Task DeleteAsync(Guid id);
    }
}
