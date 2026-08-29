using System.Net;
using System.Net.Http.Json;
using IntegrationService.Context;
using IntegrationService.Models;
using IntegrationService.Models.DTO.IntegrationDTOs;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationService.Tests.IntegrationTests
{
    public class IntegrationControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public IntegrationControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private async Task<IntegrationDisplayDTO> CreateIntegrationAsync(string type = "GitHub", string apiKey = "ghp_abcdefgh12345678")
        {
            var dto = new IntegrationCreateDTO { Type = type, ApiKey = apiKey };
            var response = await _client.PostAsJsonAsync("integrations", dto);
            response.EnsureSuccessStatusCode();
            var created = await response.Content.ReadFromJsonAsync<IntegrationDisplayDTO>();
            return created!;
        }

        [Fact]
        public async Task Post_WithValidPayload_Returns201AndNeverExposesPlainApiKey()
        {
            var response = await _client.PostAsJsonAsync(
                "integrations", new IntegrationCreateDTO { Type = "GitHub", ApiKey = "ghp_abcdefgh12345678" });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<IntegrationDisplayDTO>();
            Assert.Equal("GitHub", body!.Type);
            Assert.True(body.Status);
            Assert.DoesNotContain("ghp_abcdefgh12345678", body.ApiKeyMasked);
            Assert.EndsWith("5678", body.ApiKeyMasked);
        }

        [Fact]
        public async Task Post_WithTooShortApiKey_ReturnsBadRequest()
        {
            var response = await _client.PostAsJsonAsync(
                "integrations", new IntegrationCreateDTO { Type = "GitHub", ApiKey = "short" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetById_WhenNotFound_Returns404()
        {
            var response = await _client.GetAsync($"integrations/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetAll_ReturnsCreatedIntegrations()
        {
            await CreateIntegrationAsync(type: "Slack", apiKey: "xoxb-plaintext-secret-1");

            var response = await _client.GetAsync("integrations");

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<IntegrationDisplayDTO>>();
            Assert.Contains(result!, i => i.Type == "Slack");
        }

        [Fact]
        public async Task Put_WithoutNewApiKey_KeepsMaskSameButUpdatesTypeAndStatus()
        {
            var created = await CreateIntegrationAsync();

            var response = await _client.PutAsJsonAsync(
                $"integrations/{created.Id}",
                new IntegrationUpdateDTO { Type = "GitHub-Renamed", ApiKey = null, Status = false });

            response.EnsureSuccessStatusCode();
            var updated = await response.Content.ReadFromJsonAsync<IntegrationDisplayDTO>();
            Assert.Equal("GitHub-Renamed", updated!.Type);
            Assert.False(updated.Status);
            Assert.Equal(created.ApiKeyMasked, updated.ApiKeyMasked);
        }

        [Fact]
        public async Task Put_WithNewApiKey_RotatesMaskedValue()
        {
            var created = await CreateIntegrationAsync(apiKey: "ghp_original123456");

            var response = await _client.PutAsJsonAsync(
                $"integrations/{created.Id}",
                new IntegrationUpdateDTO { Type = "GitHub", ApiKey = "ghp_brandnewkey999", Status = true });

            response.EnsureSuccessStatusCode();
            var updated = await response.Content.ReadFromJsonAsync<IntegrationDisplayDTO>();
            Assert.NotEqual(created.ApiKeyMasked, updated!.ApiKeyMasked);
            Assert.EndsWith("999", updated.ApiKeyMasked);
        }

        [Fact]
        public async Task GetAll_WhenOneRowHasUndecryptableKey_StillReturnsTheOthers()
        {
            // Simulates a row whose ApiKeyEncrypted was produced by a key ring that's since
            // been rotated away or lost (e.g. a different environment's key store) - the
            // ciphertext is on-disk garbage as far as the current Data Protection key ring
            // is concerned, so Unprotect throws for this row specifically.
            await CreateIntegrationAsync(type: "Healthy", apiKey: "healthy-plaintext-key-1");

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<IntegrationServiceContext>();
                context.Integrations.Add(new Integration
                {
                    Id = Guid.NewGuid(),
                    Type = "Corrupted",
                    ApiKeyEncrypted = "not-a-real-protected-payload",
                    Status = true,
                    CreatedAt = DateTime.UtcNow,
                });
                await context.SaveChangesAsync();
            }

            var response = await _client.GetAsync("integrations");

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<IntegrationDisplayDTO>>();
            Assert.Contains(result!, i => i.Type == "Healthy" && i.ApiKeyMasked.EndsWith("ey-1"));
            Assert.Contains(result!, i => i.Type == "Corrupted" && i.ApiKeyMasked == "unavailable");
        }

        [Fact]
        public async Task Put_WhenNotFound_Returns404()
        {
            var response = await _client.PutAsJsonAsync(
                $"integrations/{Guid.NewGuid()}",
                new IntegrationUpdateDTO { Type = "X", Status = true });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_RemovesIntegration_SubsequentGetReturns404()
        {
            var created = await CreateIntegrationAsync();

            var deleteResponse = await _client.DeleteAsync($"integrations/{created.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var getResponse = await _client.GetAsync($"integrations/{created.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task Delete_WhenNotFound_Returns404()
        {
            var response = await _client.DeleteAsync($"integrations/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
