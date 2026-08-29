// The backend serializes these enums as plain numbers (no JsonStringEnumConverter
// is configured on SprintService/WorkPackageService), so the frontend needs its
// own label lists in the exact same order as the C# enum declarations.

// SprintService.Models.Enums.SprintStatus
export const SPRINT_STATUSES = ['NotStarted', 'Active', 'Completed', 'Cancelled'];

// WorkPackageService.Models.Enums.TaskStatus
export const TASK_STATUSES = ['ToDo', 'InProgress', 'InReview', 'Done', 'Blocked'];

// WorkPackageService.Models.Enums.TaskPriority
export const TASK_PRIORITIES = ['Low', 'Medium', 'High', 'Critical'];

export function labelFor(list, value) {
  return list[value] ?? 'Unknown';
}
