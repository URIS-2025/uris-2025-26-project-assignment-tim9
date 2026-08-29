using System.Security.Cryptography;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using IntegrationService.Data;
using IntegrationService.Exceptions;
using IntegrationService.Models;
using IntegrationService.Models.DTO.IntegrationDTOs;
using IntegrationService.Security;

namespace IntegrationService.Controllers
{
    [ApiController]
    [Route("integrations")]
    public class IntegrationController : ControllerBase
    {
        private readonly IIntegrationRepository _repository;
        private readonly IMapper _mapper;
        private readonly IApiKeyProtector _apiKeyProtector;

        public IntegrationController(
            IIntegrationRepository repository,
            IMapper mapper,
            IApiKeyProtector apiKeyProtector)
        {
            _repository = repository;
            _mapper = mapper;
            _apiKeyProtector = apiKeyProtector;
        }

        // POST /integrations
        [HttpPost]
        public async Task<ActionResult<IntegrationDisplayDTO>> Create([FromBody] IntegrationCreateDTO dto)
        {
            var integration = _mapper.Map<Integration>(dto);
            integration.ApiKeyEncrypted = _apiKeyProtector.Protect(dto.ApiKey);

            var created = await _repository.CreateAsync(integration);

            var result = ToDisplayDTO(created, dto.ApiKey);
            return CreatedAtAction(nameof(GetById), new { integrationId = result.Id }, result);
        }

        // GET /integrations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IntegrationDisplayDTO>>> GetAll()
        {
            var integrations = await _repository.GetAllAsync();
            var result = integrations.Select(ToDisplayDTOFromStorage);
            return Ok(result);
        }

        // GET /integrations/{integrationId}
        [HttpGet("{integrationId}")]
        public async Task<ActionResult<IntegrationDisplayDTO>> GetById(Guid integrationId)
        {
            var integration = await _repository.GetByIdAsync(integrationId);
            if (integration == null)
            {
                return NotFound();
            }

            return Ok(ToDisplayDTOFromStorage(integration));
        }

        // PUT /integrations/{integrationId}
        [HttpPut("{integrationId}")]
        public async Task<ActionResult<IntegrationDisplayDTO>> Update(Guid integrationId, [FromBody] IntegrationUpdateDTO dto)
        {
            var rotateApiKey = !string.IsNullOrWhiteSpace(dto.ApiKey);

            var updatedEntity = _mapper.Map<Integration>(dto);
            if (rotateApiKey)
            {
                updatedEntity.ApiKeyEncrypted = _apiKeyProtector.Protect(dto.ApiKey!);
            }

            try
            {
                var updated = await _repository.UpdateAsync(integrationId, updatedEntity, rotateApiKey);
                // A rotated key is already known in plaintext here (dto.ApiKey) - no need to
                // decrypt what was just encrypted. Otherwise fall back to the same
                // decrypt-from-storage path GetAll/GetById use, so a row whose key can't be
                // recovered doesn't turn a routine rename/status toggle into a 500.
                var result = rotateApiKey ? ToDisplayDTO(updated, dto.ApiKey!) : ToDisplayDTOFromStorage(updated);
                return Ok(result);
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
        }

        // DELETE /integrations/{integrationId}
        [HttpDelete("{integrationId}")]
        public async Task<IActionResult> Delete(Guid integrationId)
        {
            try
            {
                await _repository.DeleteAsync(integrationId);
                return NoContent();
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
        }

        private IntegrationDisplayDTO ToDisplayDTO(Integration integration, string plainApiKey)
        {
            var dto = _mapper.Map<IntegrationDisplayDTO>(integration);
            dto.ApiKeyMasked = _apiKeyProtector.Mask(plainApiKey);
            return dto;
        }

        // Decrypts straight from the stored ciphertext. If the key ring that encrypted this
        // particular row is gone (rotated away, or the row came from a different environment's
        // key store), the key itself is unrecoverable - but that shouldn't take down every other
        // integration in the same GetAll response.
        private IntegrationDisplayDTO ToDisplayDTOFromStorage(Integration integration)
        {
            try
            {
                return ToDisplayDTO(integration, _apiKeyProtector.Unprotect(integration.ApiKeyEncrypted));
            }
            catch (CryptographicException)
            {
                var dto = _mapper.Map<IntegrationDisplayDTO>(integration);
                dto.ApiKeyMasked = "unavailable";
                return dto;
            }
        }
    }
}
