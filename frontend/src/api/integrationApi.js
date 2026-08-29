import { apiRequest } from './httpClient';

// IntegrationController uses a flat route without an /api prefix (see PORTS.md).
// The API key never round-trips in plain text - GetAll/GetById return ApiKeyMasked,
// and Create/Update only ever send a plain key one-way, to be encrypted server-side.

export function getIntegrations(token) {
  return apiRequest('/integrations', { token }).then((r) => r || []);
}

export function getIntegrationById(integrationId, token) {
  return apiRequest(`/integrations/${integrationId}`, { token });
}

export function createIntegration(integration, token) {
  return apiRequest('/integrations', { method: 'POST', token, body: integration });
}

export function updateIntegration(integrationId, integration, token) {
  return apiRequest(`/integrations/${integrationId}`, { method: 'PUT', token, body: integration });
}

export function deleteIntegration(integrationId, token) {
  return apiRequest(`/integrations/${integrationId}`, { method: 'DELETE', token });
}
