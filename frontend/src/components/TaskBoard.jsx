import { useCallback, useEffect, useState } from 'react';
import { useAuth } from '../auth/useAuth';
import {
  getTasks,
  createTask,
  updateTaskStatus,
  deleteTask,
  TASK_PRIORITY_LABELS,
} from '../api/workPackageApi';
import './TaskBoard.css';

// TaskStatus enum (backend, integer on the wire):
// 0 ToDo, 1 InProgress, 2 InReview, 3 Done, 4 Blocked
const COLUMNS = [
  { key: 0, label: 'To Do' },
  { key: 1, label: 'In Progress' },
  { key: 3, label: 'Done' },
];

const STATUS_OPTIONS = [
  { value: 0, label: 'To Do' },
  { value: 1, label: 'In Progress' },
  { value: 2, label: 'In Review' },
  { value: 3, label: 'Done' },
  { value: 4, label: 'Blocked' },
];

// TaskPriority enum: 0 Low, 1 Medium, 2 High, 3 Critical
const PRIORITY_COLORS = {
  3: 'var(--color-status-critical)',
  2: 'var(--color-status-critical)',
  1: 'var(--color-status-in-progress)',
  0: 'var(--border)',
};

export default function TaskBoard({ workPackageId, onTaskClick }) {
  const { token, userId, role } = useAuth();
  const canManage = role === 'Admin' || role === 'ProjectManager';
  const [tasks, setTasks] = useState([]);
  const [phase, setPhase] = useState('loading');
  const [errorMessage, setErrorMessage] = useState('');

  const load = useCallback(() => {
    setPhase('loading');
    return getTasks(workPackageId, token)
      .then((data) => {
        setTasks(Array.isArray(data) ? data : []);
        setErrorMessage('');
        setPhase('ready');
      })
      .catch((error) => {
        setErrorMessage(
          error && error.status === 401
            ? 'Your session has expired. Please sign in again.'
            : 'Something went wrong while loading the tasks.',
        );
        setPhase('error');
      });
  }, [workPackageId, token]);

  useEffect(() => {
    let ignore = false;
    getTasks(workPackageId, token)
      .then((data) => {
        if (ignore) return;
        setTasks(Array.isArray(data) ? data : []);
        setErrorMessage('');
        setPhase('ready');
      })
      .catch((error) => {
        if (ignore) return;
        setErrorMessage(
          error && error.status === 401
            ? 'Your session has expired. Please sign in again.'
            : 'Something went wrong while loading the tasks.',
        );
        setPhase('error');
      });
    return () => {
      ignore = true;
    };
  }, [workPackageId, token]);

  async function handleAdd(columnKey) {
    const title = window.prompt('New task name:');
    if (!title || !title.trim()) return;
    try {
      await createTask(workPackageId, { title: title.trim(), status: columnKey, priority: 1 }, token);
      await load();
    } catch (error) {
      window.alert(error?.message || 'Could not create the task.');
    }
  }

  async function handleStatusChange(id, status) {
    const nextStatus = Number(status);
    const previous = tasks;
    setTasks((prev) => prev.map((task) => (task.taskId === id ? { ...task, status: nextStatus } : task)));
    try {
      await updateTaskStatus(id, nextStatus, token, userId);
    } catch (error) {
      setTasks(previous);
      window.alert(
        error && error.status === 403
          ? 'Only the person the task is assigned to can change its status.'
          : error?.message || 'Could not update the task status.',
      );
    }
  }

  async function handleDelete(id) {
    if (!window.confirm('Delete this task?')) return;
    try {
      await deleteTask(id, token);
      setTasks((prev) => prev.filter((task) => task.taskId !== id));
    } catch (error) {
      window.alert(
        error && error.status === 409
          ? 'This task still has dependencies or subtasks. Remove them first.'
          : error?.message || 'Could not delete the task.',
      );
    }
  }

  if (phase === 'loading') {
    return (
      <div className="task-board" data-work-package-id={workPackageId}>
        <h2>Task Board</h2>
        <p>Loading...</p>
      </div>
    );
  }

  if (phase === 'error') {
    return (
      <div className="task-board" data-work-package-id={workPackageId}>
        <h2>Task Board</h2>
        <p>{errorMessage}</p>
      </div>
    );
  }

  return (
    <div className="task-board" data-work-package-id={workPackageId}>
      <h2>Task Board</h2>

      <div className="task-board__columns">
        {COLUMNS.map((column) => (
          <div key={column.key} className="task-column">
            <div className="task-column__header">
              <h3>{column.label}</h3>
              {canManage && (
                <button
                  type="button"
                  className="task-column__add"
                  aria-label={`Add task to ${column.label} column`}
                  onClick={() => handleAdd(column.key)}
                >
                  +
                </button>
              )}
            </div>

            {tasks
              .filter((task) => task.status === column.key)
              .map((task) => (
                <div
                  key={task.taskId}
                  className="task-card"
                  style={{ borderLeftColor: PRIORITY_COLORS[task.priority] ?? 'var(--border)' }}
                  onClick={() => onTaskClick?.({ ...task, id: task.taskId })}
                >
                  {canManage && (
                    <button
                      type="button"
                      className="task-card__delete"
                      aria-label="Delete task"
                      onClick={(event) => {
                        event.stopPropagation();
                        handleDelete(task.taskId);
                      }}
                    >
                      ×
                    </button>
                  )}

                  <p className="task-card__title">{task.title}</p>
                  <p className="task-card__meta">
                    {task.assigneeId ? `${task.assigneeId.slice(0, 8)}…` : 'Unassigned'} •{' '}
                    {TASK_PRIORITY_LABELS[task.priority] ?? task.priority}
                  </p>

                  <select
                    className="task-card__status"
                    value={task.status}
                    onClick={(event) => event.stopPropagation()}
                    onChange={(event) => handleStatusChange(task.taskId, event.target.value)}
                  >
                    {STATUS_OPTIONS.map((option) => (
                      <option key={option.value} value={option.value}>
                        {option.label}
                      </option>
                    ))}
                  </select>
                </div>
              ))}
          </div>
        ))}
      </div>
    </div>
  );
}
