// Turns an ApiError (see api/httpClient.js - it carries `.status` and `.message`)
// into a short, friendly sentence for the UI.
//
// `context` names the action being attempted so 4xx codes can be phrased for
// that specific case. The rules below mirror what the WorkPackageService
// controllers/repositories actually enforce:
//   - POST /api/comment is [Authorize] with no role and no author/assignee
//     check, so a comment create only ever fails with 401 (auth) or 400
//     (validation) - never 403.
//   - PUT/DELETE /api/comment 403 => caller is not the comment's AuthorId.
//   - PATCH /api/task/{id}/status 403 => caller is not the task's AssigneeId.
//   - task create/delete/reassign/move, dependency writes, work-package writes
//     and backlog writes are all [Authorize(Roles = "ProjectManager,Admin")],
//     so their 403 means "not a PM/Admin".

const PM_ADMIN_ONLY = 'Only a project manager or admin can do this.';

// Per-context overrides keyed by HTTP status code.
const CONTEXT_MESSAGES = {
  'comment-create': {
    400: "Your comment couldn't be posted. Please check the text and try again.",
  },
  'comment-edit': {
    403: 'Only the author of a comment can edit or delete it.',
  },
  'task-status': {
    403: "Only the person this task is assigned to can change its status.",
  },
  'task-write': { 403: PM_ADMIN_ONLY },
  'dependency-write': { 403: PM_ADMIN_ONLY },
  'work-package-write': { 403: PM_ADMIN_ONLY },
  'backlog-write': { 403: PM_ADMIN_ONLY },
};

// Fallbacks used whenever the context has nothing more specific to say.
const GENERIC_MESSAGES = {
  401: 'Your session has expired. Please log in again.',
  403: "You don't have permission to do this.",
  404: 'This item no longer exists.',
  409: 'This action conflicts with existing data (e.g. dependent items still exist).',
};

const FALLBACK = 'Something went wrong. Please try again.';

// httpClient produces this exact string when the error response had no body -
// it's just the status code in words, so treat it as "no real message".
const RAW_STATUS_MESSAGE = /^request failed with status \d+$/i;

export function getFriendlyErrorMessage(error, context) {
  const status = typeof error?.status === 'number' ? error.status : null;
  const rawMessage = typeof error?.message === 'string' ? error.message.trim() : '';
  const hasRealMessage = rawMessage.length > 0 && !RAW_STATUS_MESSAGE.test(rawMessage);

  const perContext = (context && CONTEXT_MESSAGES[context]) || {};

  // Validation errors: a specific message from the backend is the most helpful
  // thing we can show, so prefer it over any canned text.
  if (status === 400) {
    if (hasRealMessage) return rawMessage;
    return perContext[400] || FALLBACK;
  }

  if (status != null && perContext[status]) return perContext[status];
  if (status != null && GENERIC_MESSAGES[status]) return GENERIC_MESSAGES[status];
  if (status != null && status >= 500) return FALLBACK;

  if (hasRealMessage) return rawMessage;
  return FALLBACK;
}
