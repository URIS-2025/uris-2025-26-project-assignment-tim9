import { apiRequest } from './httpClient';

// NotificationController uses a flat route without an /api prefix (see PORTS.md) -
// WorkPackageService already calls POST /notifications with this exact contract,
// so the route can't change here without breaking that caller.

export function getNotifications(userId, token) {
  return apiRequest('/notifications', { token, query: { userId } }).then((r) => r || []);
}

export function markNotificationAsRead(notificationId, token) {
  return apiRequest(`/notifications/${notificationId}`, { method: 'PUT', token });
}
