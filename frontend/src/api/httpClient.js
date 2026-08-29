// Thin fetch wrapper shared by every API module.
//
// Every single request - auth, projects, work packages, tasks, sprints -
// is sent to the API Gateway. The frontend never addresses a microservice
// directly.
//
// By default this stays relative (empty base) and lets Vite's dev-server
// proxy (see vite.config.js) forward /api, /sprints, /projects to the
// gateway - the browser never makes a cross-origin request, so there's no
// need for the gateway to have a CORS policy. Set VITE_API_BASE_URL only if
// you want to bypass the proxy and hit a gateway directly (that gateway
// would then need CORS enabled for this origin).
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '';

export class ApiError extends Error {
  constructor(status, message) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
  }
}

function messagesFromErrors(errors) {
  if (!errors || typeof errors !== 'object') return [];
  return Object.entries(errors).flatMap(([field, value]) => {
    const list = Array.isArray(value) ? value : [value];
    return list
      .map((entry) => String(entry).trim())
      .filter(Boolean)
      .map((msg) => {
        const named = Boolean(field) && field !== '$';
        const alreadyMentions = named && msg.toLowerCase().includes(field.toLowerCase());
        return named && !alreadyMentions ? `${field}: ${msg}` : msg;
      });
  });
}

async function parseErrorMessage(response) {
  const text = await response.text().catch(() => '');
  if (!text) {
    return `Request failed with status ${response.status}`;
  }
  try {
    const data = JSON.parse(text);
    if (typeof data === 'string') return data;

    const fieldMessages = messagesFromErrors(data?.errors);
    if (fieldMessages.length) return fieldMessages.join(' ');

    if (data?.detail) return data.detail;
    if (data?.message) return data.message;
    if (data?.title) return data.title;
    return text;
  } catch {
    return text;
  }
}

/**
 * @param {string} path - path relative to the gateway, e.g. "/api/timelog"
 * @param {object} [options]
 * @param {string} [options.method]
 * @param {object} [options.body]
 * @param {string} [options.token] - JWT access token, sent as Authorization: Bearer
 * @param {string} [options.userId] - sent as X-User-Id (required by TimelogService writes)
 * @param {Record<string,string>} [options.query]
 */
export async function apiRequest(path, options = {}) {
  const { method = 'GET', body, token, userId, query } = options;

  const url = new URL(path, API_BASE_URL || window.location.origin);
  if (query) {
    Object.entries(query).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        url.searchParams.set(key, value);
      }
    });
  }

  const headers = {};
  if (body !== undefined) headers['Content-Type'] = 'application/json';
  if (token) headers['Authorization'] = `Bearer ${token}`;
  if (userId) headers['X-User-Id'] = userId;

  let response;
  try {
    response = await fetch(url, {
      method,
      headers,
      body: body !== undefined ? JSON.stringify(body) : undefined,
    });
  } catch {
    throw new ApiError(0, 'Could not reach the API Gateway. Is it running on localhost:8080?');
  }

  if (response.status === 204) {
    return null;
  }

  if (!response.ok) {
    const message = await parseErrorMessage(response);
    throw new ApiError(response.status, message);
  }

  const text = await response.text();
  if (!text) return null;
  return JSON.parse(text);
}
