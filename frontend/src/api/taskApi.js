import { apiRequest } from './httpClient';

// GET /api/task/workpackage/{workPackageId} (via gateway) -> WorkPackageService
export function getTasksByWorkPackage(workPackageId, token) {
  return apiRequest(`/api/task/workpackage/${workPackageId}`, { token }).then((r) => r || []);
}

// GET /api/task/sprint/{sprintId} (via gateway) -> WorkPackageService
// Task.SprintId is a plain scalar Guid into SprintService's own DB (no EF
// foreign key, same treatment as AssigneeId/UserService) - see WorkPackageService's Task.cs.
export function getTasksBySprint(sprintId, token) {
  return apiRequest(`/api/task/sprint/${sprintId}`, { token }).then((r) => r || []);
}

// GET /api/task/{id} (via gateway) -> WorkPackageService
export function getTaskById(taskId, token) {
  return apiRequest(`/api/task/${taskId}`, { token });
}

// POST /api/task (via gateway) -> WorkPackageService
// Requires ProjectManager/Admin. sprintId is optional - omit it to create a
// task that belongs to the work package but isn't scheduled into a sprint.
export function createTask({ workPackageId, sprintId, title, description, status, priority, dueDate }, token) {
  return apiRequest('/api/task', {
    method: 'POST',
    token,
    body: { workPackageId, sprintId, title, description, status, priority, dueDate },
  });
}

// PUT /api/task (via gateway) -> WorkPackageService
// Requires ProjectManager/Admin. All fields but id are optional - only send
// what's actually being changed.
export function updateTask({ id, title, description, status, priority, assigneeId, approverId, sprintId, dueDate }, token) {
  return apiRequest('/api/task', {
    method: 'PUT',
    token,
    body: { id, title, description, status, priority, assigneeId, approverId, sprintId, dueDate },
  });
}

// DELETE /api/task/{id} (via gateway) -> WorkPackageService
// Requires ProjectManager/Admin.
export function deleteTask(taskId, token) {
  return apiRequest(`/api/task/${taskId}`, { method: 'DELETE', token });
}
