import { useEffect, useState } from 'react';
import { useAuth } from '../auth/useAuth';
import { getSubTasks, TASK_STATUS_LABELS } from '../api/workPackageApi';

// Backend supports one level of hierarchy per call via Task.ParentTaskId and
// GET /api/task/parent/{parentTaskId}. The tree is built by having each node
// lazily fetch its own children on mount.

const STATUS_COLORS = {
  Done: 'var(--color-status-done)',
  InProgress: 'var(--color-status-in-progress)',
  InReview: 'var(--color-status-in-progress)',
  ToDo: 'var(--border)',
  Blocked: 'var(--color-status-critical)',
};

function SubTaskItem({ task, token, depth = 0 }) {
  const [children, setChildren] = useState([]);

  useEffect(() => {
    let ignore = false;
    getSubTasks(task.taskId, token)
      .then((data) => {
        if (!ignore) setChildren(Array.isArray(data) ? data : []);
      })
      .catch(() => {
        if (!ignore) setChildren([]);
      });
    return () => {
      ignore = true;
    };
  }, [task.taskId, token]);

  const statusLabel = TASK_STATUS_LABELS[task.status] ?? String(task.status);

  return (
    <div style={{ marginLeft: depth * 20, marginTop: '6px' }}>
      <div
        style={{
          display: 'flex',
          alignItems: 'center',
          gap: '8px',
          border: '1px solid var(--border)',
          borderLeft: `4px solid ${STATUS_COLORS[statusLabel] ?? 'var(--border)'}`,
          borderRadius: '6px',
          padding: '8px 12px',
          background: 'var(--bg)',
          textAlign: 'left',
        }}
      >
        <span>{task.title}</span>
        <span style={{ fontSize: '12px', color: 'var(--text)' }}>({statusLabel})</span>
      </div>
      {children.length > 0 && (
        <div>
          {children.map((child) => (
            <SubTaskItem key={child.taskId} task={child} token={token} depth={depth + 1} />
          ))}
        </div>
      )}
    </div>
  );
}

export default function SubTaskTree({ taskId }) {
  const { token } = useAuth();
  const [roots, setRoots] = useState([]);
  const [phase, setPhase] = useState('loading');

  useEffect(() => {
    let ignore = false;
    getSubTasks(taskId, token)
      .then((data) => {
        if (ignore) return;
        setRoots(Array.isArray(data) ? data : []);
        setPhase('ready');
      })
      .catch(() => {
        if (!ignore) setPhase('error');
      });
    return () => {
      ignore = true;
    };
  }, [taskId, token]);

  return (
    <div>
      <h2>Sub-Tasks</h2>
      <div style={{ maxWidth: '600px', margin: '0 auto' }}>
        {phase === 'loading' && <p>Loading...</p>}
        {phase === 'error' && <p>Something went wrong while loading the sub-tasks.</p>}
        {phase === 'ready' && roots.length === 0 && <p>No sub-tasks.</p>}
        {phase === 'ready' &&
          roots.map((task) => <SubTaskItem key={task.taskId} task={task} token={token} depth={0} />)}
      </div>
    </div>
  );
}
