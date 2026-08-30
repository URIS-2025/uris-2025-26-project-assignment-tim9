import { useCallback, useEffect, useMemo, useState } from 'react';
import { useAuth } from '../auth/useAuth';
import {
  getTasks,
  createTask,
  updateTaskStatus,
  deleteTask,
  getWorkPackage,
  TASK_PRIORITY_LABELS,
} from '../api/workPackageApi';
import { updateTask } from '../api/taskApi';
import { getProjectMembersByProjectId } from '../api/projectApi';
import { useUserNames, shortId } from '../utils/userNames';
import { getFriendlyErrorMessage } from '../utils/errorMessages';
import { useToast } from '../shared/components/useToast';
import Modal from './Modal';
import './TaskBoard.css';

// TaskStatus enum (backend, integer on the wire):
// 0 ToDo, 1 InProgress, 2 InReview, 3 Done, 4 Blocked
const COLUMNS = [
  { key: 0, label: 'To Do' },
  { key: 1, label: 'In Progress' },
  { key: 2, label: 'In Review' },
  { key: 3, label: 'Done' },
  { key: 4, label: 'Blocked' },
];

const STATUS_OPTIONS = COLUMNS.map((c) => ({ value: c.key, label: c.label }));

// TaskPriority enum: 0 Low, 1 Medium, 2 High, 3 Critical
const PRIORITY_OPTIONS = [
  { value: 0, label: 'Low' },
  { value: 1, label: 'Medium' },
  { value: 2, label: 'High' },
  { value: 3, label: 'Critical' },
];

const PRIORITY_COLORS = {
  3: 'var(--color-status-critical)',
  2: 'var(--color-status-critical)',
  1: 'var(--color-status-in-progress)',
  0: 'var(--border)',
};

function TaskCardForm({ title, initial, members = [], onClose, onSubmit }) {
  const [form, setForm] = useState({
    title: initial?.title ?? '',
    description: initial?.description ?? '',
    priority: initial?.priority ?? 1,
    assigneeId: initial?.assigneeId ?? '',
  });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  async function submit(event) {
    event.preventDefault();
    if (!form.title.trim()) return;
    setSaving(true);
    setError('');
    try {
      await onSubmit({
        title: form.title.trim(),
        description: form.description.trim() || null,
        priority: Number(form.priority),
        assigneeId: form.assigneeId || null,
      });
      onClose();
    } catch (err) {
      setError(getFriendlyErrorMessage(err, 'task-write'));
      setSaving(false);
    }
  }

  return (
    <Modal title={title} onClose={saving ? () => {} : onClose}>
      <form className="task-form" onSubmit={submit}>
        <label>
          Title
          <input
            type="text"
            value={form.title}
            onChange={(e) => setForm((f) => ({ ...f, title: e.target.value }))}
            required
          />
        </label>
        <label>
          Description
          <textarea
            rows={3}
            value={form.description}
            onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
          />
        </label>
        <label>
          Priority
          <select
            value={form.priority}
            onChange={(e) => setForm((f) => ({ ...f, priority: e.target.value }))}
          >
            {PRIORITY_OPTIONS.map((o) => (
              <option key={o.value} value={o.value}>
                {o.label}
              </option>
            ))}
          </select>
        </label>
        <label>
          Assignee
          <select
            value={form.assigneeId}
            onChange={(e) => setForm((f) => ({ ...f, assigneeId: e.target.value }))}
          >
            <option value="">Unassigned</option>
            {initial?.assigneeId &&
              !members.some((m) => m.userId === initial.assigneeId) && (
                <option value={initial.assigneeId}>{shortId(initial.assigneeId)}</option>
              )}
            {members.map((m) => (
              <option key={m.userId} value={m.userId}>
                {m.username || shortId(m.userId)}
              </option>
            ))}
          </select>
        </label>
        {error && <p className="task-form__error">{error}</p>}
        <div className="task-form__actions">
          <button type="submit" className="task-form__save" disabled={saving}>
            {saving ? 'Saving...' : 'Save'}
          </button>
          <button
            type="button"
            className="task-form__cancel"
            onClick={onClose}
            disabled={saving}
          >
            Cancel
          </button>
        </div>
      </form>
    </Modal>
  );
}

export default function TaskBoard({ workPackageId, onTaskClick }) {
  const { token, userId, role } = useAuth();
  const { showToast } = useToast();
  const canManage = role === 'Admin' || role === 'ProjectManager';
  const [tasks, setTasks] = useState([]);
  const [phase, setPhase] = useState('loading');
  const [errorMessage, setErrorMessage] = useState('');
  const [addColumn, setAddColumn] = useState(null); // status key for the create modal
  const [editTask, setEditTask] = useState(null);
  const [members, setMembers] = useState([]);

  const load = useCallback(() => {
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

  useEffect(() => {
    let ignore = false;
    getWorkPackage(workPackageId, token)
      .then((wp) => (wp?.projectId ? getProjectMembersByProjectId(wp.projectId, token) : []))
      .then((list) => {
        if (!ignore) setMembers(Array.isArray(list) ? list : []);
      })
      .catch(() => {
        if (!ignore) setMembers([]);
      });
    return () => {
      ignore = true;
    };
  }, [workPackageId, token]);

  const assigneeIds = useMemo(() => tasks.map((t) => t.assigneeId), [tasks]);
  const nameFor = useUserNames(assigneeIds, token);

  async function handleStatusChange(id, status) {
    const nextStatus = Number(status);
    const previous = tasks;
    setTasks((prev) => prev.map((task) => (task.taskId === id ? { ...task, status: nextStatus } : task)));
    try {
      await updateTaskStatus(id, nextStatus, token, userId);
    } catch (error) {
      setTasks(previous);
      showToast(getFriendlyErrorMessage(error, 'task-status'), 'error');
    }
  }

  async function handleDelete(id) {
    if (!window.confirm('Delete this task?')) return;
    try {
      await deleteTask(id, token);
      setTasks((prev) => prev.filter((task) => task.taskId !== id));
      showToast('Task deleted.', 'success');
    } catch (error) {
      showToast(getFriendlyErrorMessage(error, 'task-write'), 'error');
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
                  onClick={() => setAddColumn(column.key)}
                >
                  +
                </button>
              )}
            </div>

            {tasks
              // Only top-level tasks belong on the board; sub-tasks (parentTaskId
              // set) are reachable through their parent's Sub-tasks section.
              .filter((task) => task.status === column.key && !task.parentTaskId)
              .map((task) => (
                <div
                  key={task.taskId}
                  className="task-card"
                  style={{ borderLeftColor: PRIORITY_COLORS[task.priority] ?? 'var(--border)' }}
                  onClick={() => onTaskClick?.({ ...task, id: task.taskId })}
                >
                  {canManage && (
                    <div className="task-card__actions">
                      <button
                        type="button"
                        className="task-card__edit"
                        aria-label="Edit task"
                        onClick={(event) => {
                          event.stopPropagation();
                          setEditTask(task);
                        }}
                      >
                        ✎
                      </button>
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
                    </div>
                  )}

                  <p className="task-card__title">{task.title}</p>
                  <p className="task-card__meta">
                    {task.assigneeId ? nameFor(task.assigneeId) : 'Unassigned'} •{' '}
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

      {addColumn !== null && (
        <TaskCardForm
          title="New Task"
          initial={{ priority: 1 }}
          members={members}
          onClose={() => setAddColumn(null)}
          onSubmit={async (data) => {
            await createTask(workPackageId, { ...data, status: addColumn }, token);
            await load();
          }}
        />
      )}

      {editTask && (
        <TaskCardForm
          title="Edit Task"
          initial={editTask}
          members={members}
          onClose={() => setEditTask(null)}
          onSubmit={async (data) => {
            await updateTask({ id: editTask.taskId, ...data }, token);
            await load();
          }}
        />
      )}
    </div>
  );
}
