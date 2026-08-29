import { apiRequest } from './httpClient';

export function login(username, password) {
  return apiRequest('/api/auth/login', { method: 'POST', body: { username, password } });
}

export function refreshToken(refreshTokenValue) {
  return apiRequest('/api/auth/refresh', {
    method: 'POST',
    body: { refreshToken: refreshTokenValue },
  });
}

export function logout(refreshTokenValue) {
  return apiRequest('/api/auth/logout', {
    method: 'POST',
    body: { refreshToken: refreshTokenValue },
  });
}

export function getSessions(userId, token) {
  return apiRequest(`/api/auth/sessions/${userId}`, { token }).then((r) => r || []);
}
