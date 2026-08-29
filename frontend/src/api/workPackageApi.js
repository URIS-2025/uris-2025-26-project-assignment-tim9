import { apiRequest } from './httpClient';

// --- Enum maps -------------------------------------------------------------
// WorkPackageService has NO JsonStringEnumConverter registered, so every enum
// crosses the wire as its integer value. These maps convert between the
// integer the backend speaks and the string label the UI shows.

export const WORK_PACKAGE_STATUS_LABELS = ['Planned', 'InProgress', 'OnHold', 'Completed', 'Cancelled'];
export const TASK_STATUS_LABELS = ['ToDo', 'InProgress', 'InReview', 'Done', 'Blocked'];
export const TASK_PRIORITY_LABELS = ['Low', 'Medium', 'High', 'Critical'];

export const TASK_STATUS = { ToDo: 0, InProgress: 1, InReview: 2, Done: 3, Blocked: 4 };
export const WORK_PACKAGE_STATUS = { Planned: 0, InProgress: 1, OnHold: 2, Completed: 3, Cancelled: 4 };

function toInt(value) {
  return typeof value === 'number' ? value : Number(value);
}

// --- Work packages -------------------------------------------------------
// Backend: WorkPackageController, [Route("api/[controller]")] -> /api/workpackage
// Gateway: /api/workpackage/{**catch-all} -> workpackage-cluster

export function getWorkPackages(projectId, token) {
  // GET /api/workpackage/project/{projectId}
  return apiRequest(`/api/workpackage/project/${projectId}`, { token }).then((r) => r || []);
}

export function getWorkPackage(workPackageId, token) {
  // GET /api/workpackage/{id}
  return apiRequest(`/api/workpackage/${workPackageId}`, { token });
}

export function updateWorkPackageStatus(workPackageId, status, token) {
  // PUT /api/workpackage  body: WorkPackageUpdateDTO { Id, Status? } (no PATCH endpoint exists)
  return apiRequest('/api/workpackage', {
    method: 'PUT',
    token,
    body: { id: workPackageId, status: toInt(status) },
  });
}

export function deleteWorkPackage(workPackageId, token) {
  // DELETE /api/workpackage/{id}  (Roles = ProjectManager, Admin)
  return apiRequest(`/api/workpackage/${workPackageId}`, { method: 'DELETE', token });
}

// --- Tasks --------------------------------------------------------------
// Backend: TaskController -> /api/task
// Task lives in the same backend (WorkPackageService), so its client stays here.

export function getTasks(workPackageId, token) {
  // GET /api/task/workpackage/{workPackageId}
  return apiRequest(`/api/task/workpackage/${workPackageId}`, { token }).then((r) => r || []);
}

// Kept for existing callers (TimelogsPage, TimelogFormModal).
export function getTasksByWorkPackage(workPackageId, token) {
  return getTasks(workPackageId, token);
}

export function getTask(taskId, token) {
  // GET /api/task/{id}
  return apiRequest(`/api/task/${taskId}`, { token });
}

// Kept for existing callers.
export function getTaskById(taskId, token) {
  return getTask(taskId, token);
}

export function getSubTasks(parentTaskId, token) {
  // GET /api/task/parent/{parentTaskId}
  return apiRequest(`/api/task/parent/${parentTaskId}`, { token }).then((r) => r || []);
}

export function createTask(workPackageId, data, token) {
  // POST /api/task  body: TaskCreateDTO { WorkPackageId, Title, Status, Priority, ParentTaskId?, ... }
  // (Roles = ProjectManager, Admin)
  return apiRequest('/api/task', {
    method: 'POST',
    token,
    body: {
      workPackageId,
      title: data.title,
      description: data.description ?? null,
      status: toInt(data.status ?? 0),
      priority: toInt(data.priority ?? 1),
      parentTaskId: data.parentTaskId ?? null,
      assigneeId: data.assigneeId ?? null,
    },
  });
}

export function updateTaskStatus(taskId, status, token, callerId) {
  // PATCH /api/task/{id}/status?callerId={guid}  body: { NewStatus }
  // Backend rule: only the task's AssigneeId may change the status (403 otherwise).
  return apiRequest(`/api/task/${taskId}/status`, {
    method: 'PATCH',
    token,
    query: { callerId },
    body: { newStatus: toInt(status) },
  });
}

export function deleteTask(taskId, token) {
  // DELETE /api/task/{id}  (Roles = ProjectManager, Admin)
  // 409 Conflict if the task still has dependencies/subtasks.
  return apiRequest(`/api/task/${taskId}`, { method: 'DELETE', token });
}

// --- Comments ---------------------------------------------------------
// Backend: CommentController -> /api/comment

export function getComments(taskId, token) {
  // GET /api/comment/task/{taskId}
  return apiRequest(`/api/comment/task/${taskId}`, { token }).then((r) => r || []);
}

export function addComment(taskId, text, token, authorId) {
  // POST /api/comment  body: CommentCreateDTO { TaskId, AuthorId, Text }
  return apiRequest('/api/comment', {
    method: 'POST',
    token,
    body: { taskId, authorId, text },
  });
}

// --- Dependencies ---------------------------------------------------
// Backend: DependencyController -> /api/dependency

export function getDependencies(taskId, token) {
  // GET /api/dependency/task/{taskId}
  return apiRequest(`/api/dependency/task/${taskId}`, { token }).then((r) => r || []);
}

export function addDependency(taskId, blockerTaskId, token) {
  // POST /api/dependency  body: DependencyCreateDTO { TaskId, BlockerTaskId }
  // (Roles = ProjectManager, Admin)
  return apiRequest('/api/dependency', {
    method: 'POST',
    token,
    body: { taskId, blockerTaskId },
  });
}

// --- Backlog --------------------------------------------------------
// Backend: BacklogController -> /api/backlog

export function getBacklog(projectId, token) {
  // GET /api/backlog/project/{projectId}
  return apiRequest(`/api/backlog/project/${projectId}`, { token }).then((r) => r || []);
}

export function addBacklogItem(projectId, data, token, createdBy) {
  // POST /api/backlog  body: BacklogCreateDTO { ProjectId, Name, Description, CreatedBy }
  // (Roles = ProjectManager, Admin)
  return apiRequest('/api/backlog', {
    method: 'POST',
    token,
    body: {
      projectId,
      name: data.name,
      description: data.description ?? null,
      createdBy,
    },
  });
}

export function deleteBacklogItem(backlogItemId, token) {
  // DELETE /api/backlog/{id}  (Roles = ProjectManager, Admin)
  return apiRequest(`/api/backlog/${backlogItemId}`, { method: 'DELETE', token });
}

export function updateBacklogItem(backlogItemId, data, token) {
  // PUT /api/backlog  body: BacklogUpdateDTO { Id, Name?, Description? }
  // (Roles = ProjectManager, Admin)
  return apiRequest('/api/backlog', {
    method: 'PUT',
    token,
    body: { id: backlogItemId, name: data.name, description: data.description },
  });
}
