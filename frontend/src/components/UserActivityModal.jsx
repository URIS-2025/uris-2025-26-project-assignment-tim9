import { useEffect, useState } from 'react';
import { getUserActivityLog } from '../api/userApi';

function formatDate(iso) {
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return iso;
  return d.toLocaleString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

export default function UserActivityModal({ userId, token }) {
  const [logs, setLogs] = useState([]);
  const [phase, setPhase] = useState('loading');

  useEffect(() => {
    let ignore = false;
    getUserActivityLog(userId, token)
      .then((data) => {
        if (ignore) return;
        setLogs(data);
        setPhase('ready');
      })
      .catch(() => {
        if (ignore) return;
        setPhase('error');
      });
    return () => {
      ignore = true;
    };
  }, [userId, token]);

  if (phase === 'loading') return <p className="status-hint">Loading activity…</p>;
  if (phase === 'error') {
    return <p className="form-message error">Couldn't load the activity log.</p>;
  }
  if (logs.length === 0) return <p className="status-hint">No activity recorded yet.</p>;

  return (
    <ul className="activity-list">
      {logs.map((log) => (
        <li key={log.logId} className="activity-item">
          <span className="activity-action">{log.action}</span>
          <span className="activity-meta">
            {formatDate(log.timestamp)}
            {log.details ? ` — ${log.details}` : ''}
          </span>
        </li>
      ))}
    </ul>
  );
}
