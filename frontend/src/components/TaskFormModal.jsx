import { useEffect, useState } from 'react';
import Modal from './Modal';
import { useAuth } from '../auth/useAuth';
import { getWorkPackagesByProject } from '../api/workPackageApi';
import { createTask } from '../api/taskApi';
import { addMockTaskToSprint } from '../mock/sprintTaskLinks';
import { TASK_STATUSES, TASK_PRIORITIES } from '../shared/enums';

/**
 * Creates a real task via WorkPackageService, then records it as belonging
 * to `sprintId` - the sprint isn't a field the user picks, it's implicit
 * from which sprint's "+ New Task" button opened this modal. That
 * sprint<->task link is mocked locally for now: WorkPackageService's Task
 * has no SprintId yet (see the handoff spec for that service's owner).
 *
 * @param {object} props
 * @param {string} props.sprintId
 * @param {string} props.projectId - the sprint's project; scopes the work package picker
 * @param {() => void} props.onClose
 * @param {() => void} props.onCreated
 */
export default function TaskFormModal({ sprintId, projectId, onClose, onCreated }) {
  const { token } = useAuth();

  const [workPackages, setWorkPackages] = useState([]);
  const [wpLoading, setWpLoading] = useState(true);
  const [wpError, setWpError] = useState(null);

  const [workPackageId, setWorkPackageId] = useState('');
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [status, setStatus] = useState('0');
  const [priority, setPriority] = useState('1');
  const [dueDate, setDueDate] = useState('');

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      setWpLoading(true);
      setWpError(null);
      try {
        const list = await getWorkPackagesByProject(projectId, token);
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
  }, [projectId, token]);

  async function handleSubmit(e) {
    e.preventDefault();
    setError(null);

    if (!workPackageId || !title.trim()) {
      setError('Pick a work package and give the task a title.');
      return;
    }

    setSubmitting(true);
    try {
      const created = await createTask(
        {
          workPackageId,
          title: title.trim(),
          description: description.trim() || null,
          status: Number(status),
          priority: Number(priority),
          dueDate: dueDate || null,
        },
        token
      );
      // Real task, mocked sprint link (see comment above).
      addMockTaskToSprint(sprintId, created);
      onCreated();
    } catch (err) {
      setError(err.message || 'Could not create the task.');
    } finally {
      setSubmitting(false);
    }
  }

  const hasWorkPackages = workPackages.length > 0;

  return (
    <Modal title="New task" onClose={onClose}>
      {wpLoading ? (
        <p className="status-hint">Loading work packages…</p>
      ) : wpError ? (
        <div className="form-message error">{wpError}</div>
      ) : !hasWorkPackages ? (
        <div className="form-message error">
          This project has no work packages yet. Create one first, then you can add tasks to it.
        </div>
      ) : (
        <form className="stacked-form" onSubmit={handleSubmit}>
          {error && <div className="form-message error">{error}</div>}

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
              {submitting ? 'Creating…' : 'Create task'}
            </button>
          </div>
        </form>
      )}
    </Modal>
  );
}
