import { apiRequest } from './httpClient';

// GET /sprints (via gateway) -> SprintService
// All sprints, or just one project's when projectId is passed (the same
// filter SprintController's GET /projects/{projectId}/sprints applies, just
// via the query-string convenience it also accepts). For non-Client callers
// with no projectId, the backend returns every sprint system-wide (Clients
// are restricted server-side to their own projects either way).
export function getAllSprints(token, { projectId } = {}) {
  return apiRequest('/sprints', { token, query: { projectId } }).then((r) => r || []);
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

// PUT /sprints/{sprintId} (via gateway) -> SprintService
// Only Admin/ProjectManager may edit a sprint. projectId is required by the
// backend DTO even though it's rarely changed in practice - SprintService
// re-validates it exists the same way it does on create.
export function updateSprint(sprintId, { projectId, name, status, startDate, endDate }, token) {
  return apiRequest(`/sprints/${sprintId}`, {
    method: 'PUT',
    token,
    body: { projectId, name, status, startDate, endDate },
  });
}

// DELETE /sprints/{sprintId} (via gateway) -> SprintService
// Only Admin/ProjectManager may delete a sprint.
export function deleteSprint(sprintId, token) {
  return apiRequest(`/sprints/${sprintId}`, { method: 'DELETE', token });
}
