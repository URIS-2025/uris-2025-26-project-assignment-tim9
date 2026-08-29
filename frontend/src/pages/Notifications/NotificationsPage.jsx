import { useCallback, useEffect, useState } from 'react';
import { useAuth } from '../../auth/useAuth';
import { ApiError } from '../../api/httpClient';
import { getNotifications, markNotificationAsRead } from '../../api/notificationApi';
import '../../components/listControls.css';
import '../../components/rowActions.css';
import './NotificationsPage.css';

export default function NotificationsPage() {
  const { token, userId, logout } = useAuth();

  const [notifications, setNotifications] = useState([]);
  const [phase, setPhase] = useState('loading');
  const [errorMessage, setErrorMessage] = useState('');
  const [reloadKey, setReloadKey] = useState(0);
  const [filter, setFilter] = useState('all'); // 'all' | 'unread'
  const [markingId, setMarkingId] = useState(null);

  const handleAuthError = useCallback(
    (err) => {
      if (err instanceof ApiError && err.status === 401) {
        logout();
        return true;
      }
      return false;
    },
    [logout]
  );

  useEffect(() => {
    let cancelled = false;
    (async () => {
      setPhase('loading');
      setErrorMessage('');
      try {
        const data = await getNotifications(userId, token);
        if (cancelled) return;
        const sorted = [...data].sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
        setNotifications(sorted);
        setPhase('ready');
      } catch (err) {
        if (cancelled) return;
        if (!handleAuthError(err)) {
          setErrorMessage(err.message || 'Something went wrong while loading your notifications.');
          setPhase('error');
        }
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [userId, token, reloadKey, handleAuthError]);

  const reload = () => setReloadKey((k) => k + 1);

  async function handleMarkAsRead(id) {
    setMarkingId(id);
    try {
      await markNotificationAsRead(id, token);
      reload();
    } catch (err) {
      if (!handleAuthError(err)) {
        setErrorMessage(err.message || 'Could not mark this notification as read.');
      }
    } finally {
      setMarkingId(null);
    }
  }

  const visible = filter === 'unread' ? notifications.filter((n) => !n.isRead) : notifications;
  const unreadCount = notifications.filter((n) => !n.isRead).length;

  return (
    <div className="notifications-page">
      <header className="page-header">
        <div>
          <h1>Notifications</h1>
          <p className="page-subtitle">
            {unreadCount > 0 ? `${unreadCount} unread` : "You're all caught up."}
          </p>
        </div>
      </header>

      <div className="list-toolbar">
        <div className="list-chips">
          <button
            type="button"
            className={filter === 'all' ? 'list-chip is-active' : 'list-chip'}
            onClick={() => setFilter('all')}
          >
            All
          </button>
          <button
            type="button"
            className={filter === 'unread' ? 'list-chip is-active' : 'list-chip'}
            onClick={() => setFilter('unread')}
          >
            Unread
          </button>
        </div>
      </div>

      {phase === 'loading' && <p className="status-hint">Loading your notifications…</p>}

      {phase === 'error' && (
        <div className="notifications-state notifications-state--error" role="alert">
          <p>{errorMessage}</p>
          <button type="button" className="secondary-button" onClick={reload}>
            Try again
          </button>
        </div>
      )}

      {phase === 'ready' && visible.length === 0 && (
        <div className="notifications-state">
          <p>{filter === 'unread' ? 'No unread notifications.' : 'No notifications yet.'}</p>
        </div>
      )}

      {phase === 'ready' && visible.length > 0 && (
        <ul className="notifications-list">
          {visible.map((n) => (
            <li
              key={n.id}
              className={
                n.isRead ? 'notification-row' : 'notification-row notification-row--unread'
              }
            >
              <span className="notification-type">{n.type}</span>
              <p className="notification-message">{n.description}</p>
              <span className="notification-time">{new Date(n.createdAt).toLocaleString()}</span>
              {!n.isRead && (
                <button
                  type="button"
                  className="row-action"
                  disabled={markingId === n.id}
                  onClick={() => handleMarkAsRead(n.id)}
                >
                  {markingId === n.id ? 'Marking…' : 'Mark as read'}
                </button>
              )}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
