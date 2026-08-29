import { apiRequest } from './httpClient';

export function getWorkPackages(projectId, token) {
  return apiRequest(`/api/workpackage/project/${projectId}`, { token }).then((r) => r || []);
}

// Task lives in the same backend (WorkPackageService), so its client goes
// here alongside WorkPackage rather than in its own file.
export function getTaskById(taskId, token) {
  return apiRequest(`/api/task/${taskId}`, { token });
}

export function getTasksByWorkPackage(workPackageId, token) {
  return apiRequest(`/api/task/workpackage/${workPackageId}`, { token }).then((r) => r || []);
}
