import { useEffect, useState } from 'react';
import Modal from './Modal';
import { useAuth } from '../auth/useAuth';
import { getWorkPackages } from '../api/workPackageApi';
import { createTask, updateTask } from '../api/taskApi';
import { TASK_STATUSES, TASK_PRIORITIES } from '../shared/enums';

/**
 * Creates or edits a task via WorkPackageService.
 *
 * Create mode: pass `sprintId` + `projectId`, omit `task`. The sprint isn't
 * a field the user picks, it's implicit from which sprint's "+ New Task"
 * button opened this modal; the work package is asked for since that's
 * fixed at creation time.
 *
 * Edit mode: pass an existing `task` to edit it in place. Its work package
 * isn't shown or editable - TaskUpdateDTO has no WorkPackageId field, the
 * backend doesn't support moving a task to a different work package.
 *
 * @param {object} props
 * @param {string} [props.sprintId] - required when creating
 * @param {string} [props.projectId] - required when creating; scopes the work package picker
 * @param {object} [props.task] - the task being edited; omit to create instead
 * @param {() => void} props.onClose
 * @param {() => void} props.onSaved
 */
export default function TaskFormModal({ sprintId, projectId, task, onClose, onSaved }) {
  const { token } = useAuth();
  const isEditing = Boolean(task);

  const [workPackages, setWorkPackages] = useState([]);
  const [wpLoading, setWpLoading] = useState(!isEditing);
  const [wpError, setWpError] = useState(null);

  const [workPackageId, setWorkPackageId] = useState('');
  const [title, setTitle] = useState(task?.title ?? '');
  const [description, setDescription] = useState(task?.description ?? '');
  const [status, setStatus] = useState(String(task?.status ?? 0));
  const [priority, setPriority] = useState(String(task?.priority ?? 1));
  const [dueDate, setDueDate] = useState(task?.dueDate ? task.dueDate.slice(0, 10) : '');

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    if (isEditing) return undefined; // the work package is fixed once the task exists
    let cancelled = false;
    (async () => {
      setWpLoading(true);
      setWpError(null);
      try {
        const list = await getWorkPackages(projectId, token);
        if (!cancelled) setWorkPackages(list);
      } catch (err) {
        if (!cancelled) setWpError(err.message || 'Could not load work packages for this project.');
      } finally {
        if (!cancelled) setWpLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [isEditing, projectId, token]);

  async function handleSubmit(e) {
    e.preventDefault();
    setError(null);

    if (!isEditing && !workPackageId) {
      setError('Pick a work package and give the task a title.');
      return;
    }
    if (!title.trim()) {
      setError('Give the task a title.');
      return;
    }

    setSubmitting(true);
    try {
      if (isEditing) {
        await updateTask(
          {
            id: task.taskId,
            title: title.trim(),
            description: description.trim() || null,
            status: Number(status),
            priority: Number(priority),
            dueDate: dueDate || null,
          },
          token
        );
      } else {
        await createTask(
          {
            workPackageId,
            sprintId,
            title: title.trim(),
            description: description.trim() || null,
            status: Number(status),
            priority: Number(priority),
            dueDate: dueDate || null,
          },
          token
        );
      }
      onSaved();
    } catch (err) {
      setError(err.message || `Could not ${isEditing ? 'save' : 'create'} the task.`);
    } finally {
      setSubmitting(false);
    }
  }

  const hasWorkPackages = workPackages.length > 0;
  const blockedByNoWorkPackages = !isEditing && !wpLoading && !wpError && !hasWorkPackages;

  return (
    <Modal title={isEditing ? 'Edit task' : 'New task'} onClose={onClose}>
      {!isEditing && wpLoading ? (
        <p className="status-hint">Loading work packages…</p>
      ) : !isEditing && wpError ? (
        <div className="form-message error">{wpError}</div>
      ) : blockedByNoWorkPackages ? (
        <div className="form-message error">
          This project has no work packages yet. Create one first, then you can add tasks to it.
        </div>
      ) : (
        <form className="stacked-form" onSubmit={handleSubmit}>
          {error && <div className="form-message error">{error}</div>}

          {!isEditing && (
            <label>
              Work package
              <select required value={workPackageId} onChange={(e) => setWorkPackageId(e.target.value)}>
                <option value="" disabled>
                  Select a work package…
                </option>
                {workPackages.map((wp) => (
                  <option key={wp.workPackageId} value={wp.workPackageId}>
                    {wp.name}
                  </option>
                ))}
              </select>
            </label>
          )}

          <label>
            Title
            <input required value={title} onChange={(e) => setTitle(e.target.value)} />
          </label>

          <label>
            Description
            <textarea
              rows={3}
              value={description}
              onChange={(e) => setDescription(e.target.value)}
            />
          </label>

          <label>
            Status
            <select value={status} onChange={(e) => setStatus(e.target.value)}>
              {TASK_STATUSES.map((label, index) => (
                <option key={label} value={index}>
                  {label}
                </option>
              ))}
            </select>
          </label>

          <label>
            Priority
            <select value={priority} onChange={(e) => setPriority(e.target.value)}>
              {TASK_PRIORITIES.map((label, index) => (
                <option key={label} value={index}>
                  {label}
                </option>
              ))}
            </select>
          </label>

          <label>
            Due date (optional)
            <input type="date" value={dueDate} onChange={(e) => setDueDate(e.target.value)} />
          </label>

          <div className="modal-actions">
            <button type="button" className="secondary-button" onClick={onClose}>
              Cancel
            </button>
            <button type="submit" className="primary-button" disabled={submitting}>
              {submitting
                ? isEditing
                  ? 'Saving…'
                  : 'Creating…'
                : isEditing
                  ? 'Save changes'
                  : 'Create task'}
            </button>
          </div>
        </form>
      )}
    </Modal>
  );
}
