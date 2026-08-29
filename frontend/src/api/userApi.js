import { apiRequest } from './httpClient';

// UserService's GetUsers already supports server-side search across
// name/username/email via ?search=, so there's no need to fetch everyone
// and filter client-side.
export function searchUsers(query, token) {
  return apiRequest('/api/user', { token, query: { search: query } }).then((r) => r || []);
}
