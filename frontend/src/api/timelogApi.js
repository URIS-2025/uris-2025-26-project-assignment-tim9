import { apiRequest } from './httpClient';

// GET /api/timelog (via gateway) -> TimelogService
// GET /api/timelog?projectId={id}&taskId={id} - both filters are optional and
// server-side; omit either (or both) to fetch across every project/task.
// There's still no "mine only" filter, so callers filter by loggedByUserId
// client-side regardless of which project/task filters are applied.
export function getAllTimelogs(token, { projectId, taskId } = {}) {
  return apiRequest('/api/timelog', { token, query: { projectId, taskId } }).then((r) => r || []);
}

// POST /api/timelog (via gateway) -> TimelogService
// Requires X-User-Id (the acting user) and a Bearer token (forwarded to
// ProjectService/WorkPackageService for membership + existence checks).
export function createTimelog({ projectId, taskId, hoursSpent, date }, token, userId) {
  return apiRequest('/api/timelog', {
    method: 'POST',
    token,
    userId,
    body: { projectId, taskId, hoursSpent, date },
  });
}

// PUT /api/timelog/{id} (via gateway) -> TimelogService
export function updateTimelog(id, { projectId, taskId, hoursSpent, date }, token, userId) {
  return apiRequest(`/api/timelog/${id}`, {
    method: 'PUT',
    token,
    userId,
    body: { projectId, taskId, hoursSpent, date },
  });
}

// DELETE /api/timelog/{id} (via gateway) -> TimelogService
export function deleteTimelog(id, token, userId) {
  return apiRequest(`/api/timelog/${id}`, {
    method: 'DELETE',
    token,
    userId,
  });
}
