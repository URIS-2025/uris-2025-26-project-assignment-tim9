import { apiRequest } from './httpClient';

// GET /api/workpackage/project/{projectId} (via gateway) -> WorkPackageService
export function getWorkPackagesByProject(projectId, token) {
  return apiRequest(`/api/workpackage/project/${projectId}`, { token }).then((r) => r || []);
}
