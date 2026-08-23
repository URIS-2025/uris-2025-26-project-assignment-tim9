using Microsoft.EntityFrameworkCore;
using IntegrationService.Context;
using IntegrationService.Exceptions;
using IntegrationService.Models;

namespace IntegrationService.Data
{
    public class IntegrationRepository : IIntegrationRepository
    {
        private readonly IntegrationServiceContext _context;

        public IntegrationRepository(IntegrationServiceContext context)
        {
            _context = context;
        }

        public async Task<Integration> CreateAsync(Integration integration)
        {
            integration.Id = Guid.NewGuid();
            integration.CreatedAt = DateTime.UtcNow;
            integration.Status = true;

            _context.Integrations.Add(integration);
            await _context.SaveChangesAsync();

            return integration;
        }

        public async Task<IEnumerable<Integration>> GetAllAsync()
        {
            return await _context.Integrations.OrderByDescending(i => i.CreatedAt).ToListAsync();
        }

        public async Task<Integration?> GetByIdAsync(Guid id)
        {
            return await _context.Integrations.FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Integration> UpdateAsync(Guid id, Integration updated, bool rotateApiKey)
        {
            var existing = await _context.Integrations.FirstOrDefaultAsync(i => i.Id == id)
                ?? throw new EntityNotFoundException($"Integracija sa ID-jem {id} ne postoji.");

            existing.Type = updated.Type;
            existing.Status = updated.Status;
            if (rotateApiKey)
            {
                existing.ApiKeyEncrypted = updated.ApiKeyEncrypted;
            }

            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task DeleteAsync(Guid id)
        {
            var existing = await _context.Integrations.FirstOrDefaultAsync(i => i.Id == id)
                ?? throw new EntityNotFoundException($"Integracija sa ID-jem {id} ne postoji.");

            _context.Integrations.Remove(existing);
            await _context.SaveChangesAsync();
        }
    }
}
