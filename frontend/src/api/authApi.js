import { apiRequest } from './httpClient';

// POST /api/auth/login (via gateway) -> AuthService
export function login(username, password) {
  return apiRequest('/api/auth/login', {
    method: 'POST',
    body: { username, password },
  });
}

// POST /api/user (via gateway) -> UserService self-registration.
// Handy for spinning up a fresh account against a clean docker-compose DB.
export function register({ name, username, email, contactInfo, password }) {
  return apiRequest('/api/user', {
    method: 'POST',
    body: { name, username, email, contactInfo, password },
  });
}
