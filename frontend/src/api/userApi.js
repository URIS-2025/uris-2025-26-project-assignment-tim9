import { apiRequest } from './httpClient';

// UserService's GetUsers already supports server-side search across
// name/username/email via ?search=, so there's no need to fetch everyone
// and filter client-side.
export function searchUsers(query, token) {
  return apiRequest('/api/user', { token, query: { search: query } }).then((r) => r || []);
}

export function getUsers(token, { search, role, isActive } = {}) {
  return apiRequest('/api/user', { token, query: { search, role, isActive } }).then(
    (r) => r || []
  );
}

// Public registration - no token required. New accounts always start as TeamMember.
export function registerUser(user) {
  return apiRequest('/api/user', { method: 'POST', body: user });
}

export function getUserById(userId, token) {
  return apiRequest(`/api/user/${userId}`, { token });
}

export function updateUser(userId, user, token) {
  return apiRequest(`/api/user/${userId}`, { method: 'PUT', token, body: user });
}

export function deactivateUser(userId, performedBy, token) {
  return apiRequest(`/api/user/${userId}/deactivate`, {
    method: 'PATCH',
    token,
    query: { performedBy },
  });
}

export function activateUser(userId, performedBy, token) {
  return apiRequest(`/api/user/${userId}/activate`, {
    method: 'PATCH',
    token,
    query: { performedBy },
  });
}

export function changeRole(userId, newRole, changedBy, token) {
  return apiRequest('/api/user/role', {
    method: 'PATCH',
    token,
    body: { userId, newRole, changedBy },
  });
}

export function getUserActivityLog(userId, token) {
  return apiRequest(`/api/user/${userId}/audit`, { token }).then((r) => r || []);
}
