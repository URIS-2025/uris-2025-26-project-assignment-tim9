import { useCallback, useEffect, useState } from 'react';
import { useAuth } from '../auth/useAuth';
import { getSubTasks, createTask, TASK_STATUS_LABELS } from '../api/workPackageApi';
import { getFriendlyErrorMessage } from '../utils/errorMessages';

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

const addButtonStyle = {
  padding: '3px 8px',
  fontSize: '12px',
  fontFamily: 'var(--sans)',
  borderRadius: '6px',
  border: '1px solid var(--accent)',
  background: 'var(--accent)',
  color: '#fff',
  cursor: 'pointer',
};

function AddSubTaskRow({ parentTaskId, workPackageId, onCreated }) {
  const { token } = useAuth();
  const [open, setOpen] = useState(false);
  const [title, setTitle] = useState('');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  async function submit(event) {
    event.preventDefault();
    const trimmed = title.trim();
    if (!trimmed) return;
    setSaving(true);
    setError('');
    try {
      await createTask(workPackageId, { title: trimmed, parentTaskId: parentTaskId }, token);
      setTitle('');
      setOpen(false);
      onCreated();
    } catch (err) {
      setError(getFriendlyErrorMessage(err, 'task-write'));
    } finally {
      setSaving(false);
    }
  }

  if (!open) {
    return (
      <button type="button" style={addButtonStyle} onClick={() => setOpen(true)}>
        + Add sub-task
      </button>
    );
  }

  return (
    <form onSubmit={submit} style={{ display: 'flex', gap: '6px', flexWrap: 'wrap', alignItems: 'center' }}>
      <input
        type="text"
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        placeholder="Sub-task title"
        autoFocus
        style={{
          flex: '1 1 160px',
          padding: '5px 8px',
          borderRadius: '6px',
          border: '1px solid var(--border)',
          fontFamily: 'var(--sans)',
          fontSize: '13px',
        }}
      />
      <button type="submit" style={addButtonStyle} disabled={saving}>
        {saving ? 'Adding…' : 'Add'}
      </button>
      <button
        type="button"
        onClick={() => {
          setOpen(false);
          setError('');
        }}
        style={{ ...addButtonStyle, background: 'transparent', color: 'var(--text)', borderColor: 'var(--border)' }}
      >
        Cancel
      </button>
      {error && <span style={{ fontSize: '12px', color: 'var(--color-status-critical)' }}>{error}</span>}
    </form>
  );
}

function SubTaskItem({ task, workPackageId, token, onTaskClick, depth = 0 }) {
  const [children, setChildren] = useState([]);
  const [version, setVersion] = useState(0);

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
  }, [task.taskId, token, version]);

  const statusLabel = TASK_STATUS_LABELS[task.status] ?? String(task.status);

  return (
    <div style={{ marginLeft: depth * 20, marginTop: '6px' }}>
      <div
        role="button"
        tabIndex={0}
        onClick={() => onTaskClick?.(task)}
        onKeyDown={(event) => {
          if (event.key === 'Enter' || event.key === ' ') {
            event.preventDefault();
            onTaskClick?.(task);
          }
        }}
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
          cursor: onTaskClick ? 'pointer' : 'default',
        }}
      >
        <span>{task.title}</span>
        <span style={{ fontSize: '12px', color: 'var(--text)' }}>({statusLabel})</span>
      </div>
      {children.length > 0 && (
        <div>
          {children.map((child) => (
            <SubTaskItem
              key={child.taskId}
              task={child}
              workPackageId={workPackageId}
              token={token}
              onTaskClick={onTaskClick}
              depth={depth + 1}
            />
          ))}
        </div>
      )}
      <div style={{ marginLeft: 20, marginTop: '6px' }}>
        <AddSubTaskRow
          parentTaskId={task.taskId}
          workPackageId={workPackageId}
          onCreated={() => setVersion((v) => v + 1)}
        />
      </div>
    </div>
  );
}

export default function SubTaskTree({ taskId, workPackageId, onTaskClick }) {
  const { token } = useAuth();
  const [roots, setRoots] = useState([]);
  const [phase, setPhase] = useState('loading');
  const [version, setVersion] = useState(0);

  const reload = useCallback(() => setVersion((v) => v + 1), []);

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
  }, [taskId, token, version]);

  return (
    <div>
      <h3 style={{ fontSize: '16px' }}>Sub-tasks</h3>
      <div>
        {phase === 'loading' && <p>Loading...</p>}
        {phase === 'error' && <p>Something went wrong while loading the sub-tasks.</p>}
        {phase === 'ready' && roots.length === 0 && (
          <p style={{ fontSize: '14px', color: 'var(--text)' }}>No sub-tasks.</p>
        )}
        {phase === 'ready' &&
          roots.map((task) => (
            <SubTaskItem
              key={task.taskId}
              task={task}
              workPackageId={workPackageId}
              token={token}
              onTaskClick={onTaskClick}
              depth={0}
            />
          ))}
        {workPackageId && (
          <div style={{ marginTop: '10px' }}>
            <AddSubTaskRow parentTaskId={taskId} workPackageId={workPackageId} onCreated={reload} />
          </div>
        )}
      </div>
    </div>
  );
}
