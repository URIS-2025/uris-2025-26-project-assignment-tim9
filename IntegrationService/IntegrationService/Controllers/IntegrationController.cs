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
            var result = integrations.Select(i => ToDisplayDTO(i, _apiKeyProtector.Unprotect(i.ApiKeyEncrypted)));
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

            var plainKey = _apiKeyProtector.Unprotect(integration.ApiKeyEncrypted);
            return Ok(ToDisplayDTO(integration, plainKey));
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
                var plainKey = _apiKeyProtector.Unprotect(updated.ApiKeyEncrypted);
                return Ok(ToDisplayDTO(updated, plainKey));
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
    }
}
