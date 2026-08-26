import { apiRequest } from './httpClient';

// GET /api/project (via gateway) -> ProjectService
// All projects - used to populate the "attach sprint to a project" selector,
// since creating a sprint isn't limited to projects the caller is a member of.
export function getAllProjects(token) {
  return apiRequest('/api/project', { token }).then((r) => r || []);
}

// GET /api/project/user/{userId} (via gateway) -> ProjectService
// Projects the given user is a member of.
export function getProjectsByUser(userId, token) {
  return apiRequest(`/api/project/user/${userId}`, { token }).then((r) => r || []);
}

// GET /api/project/{projectId} (via gateway) -> ProjectService
export function getProjectById(projectId, token) {
  return apiRequest(`/api/project/${projectId}`, { token });
}
