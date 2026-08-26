import { apiRequest } from './httpClient';

// GET /api/task/workpackage/{workPackageId} (via gateway) -> WorkPackageService
export function getTasksByWorkPackage(workPackageId, token) {
  return apiRequest(`/api/task/workpackage/${workPackageId}`, { token }).then((r) => r || []);
}

// NOTE: no getTasksBySprint here - WorkPackageService's Task has no SprintId
// and there is no GET /api/task/sprint/{id}. Sprint<->task association is
// mocked locally for now; see ../mock/sprintTaskLinks.js.

// GET /api/task/{id} (via gateway) -> WorkPackageService
export function getTaskById(taskId, token) {
  return apiRequest(`/api/task/${taskId}`, { token });
}

// POST /api/task (via gateway) -> WorkPackageService
// Requires ProjectManager/Admin. Real task creation - the sprint attachment
// is recorded separately (mocked) until WorkPackageService supports it.
export function createTask({ workPackageId, title, description, status, priority, dueDate }, token) {
  return apiRequest('/api/task', {
    method: 'POST',
    token,
    body: { workPackageId, title, description, status, priority, dueDate },
  });
}

// DELETE /api/task/{id} (via gateway) -> WorkPackageService
// Requires ProjectManager/Admin.
export function deleteTask(taskId, token) {
  return apiRequest(`/api/task/${taskId}`, { method: 'DELETE', token });
}
