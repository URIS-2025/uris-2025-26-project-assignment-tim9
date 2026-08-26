import { apiRequest } from './httpClient';

// GET /sprints (via gateway) -> SprintService
// All sprints. For non-Client callers the backend returns every sprint
// system-wide (Clients are restricted server-side to their own projects).
export function getAllSprints(token) {
  return apiRequest('/sprints', { token }).then((r) => r || []);
}

// POST /projects/{projectId}/sprints (via gateway) -> SprintService
// Only Admin/ProjectManager may create a sprint.
export function createSprint(projectId, { name, status, startDate, endDate }, token) {
  return apiRequest(`/projects/${projectId}/sprints`, {
    method: 'POST',
    token,
    body: { name, status, startDate, endDate },
  });
}

// DELETE /sprints/{sprintId} (via gateway) -> SprintService
// Only Admin/ProjectManager may delete a sprint.
export function deleteSprint(sprintId, token) {
  return apiRequest(`/sprints/${sprintId}`, { method: 'DELETE', token });
}
