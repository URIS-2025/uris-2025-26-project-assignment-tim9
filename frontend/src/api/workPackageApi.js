import { apiRequest } from './httpClient';

export function getWorkPackages(projectId, token) {
  return apiRequest(`/projects/${projectId}/work-packages`, { token });
}