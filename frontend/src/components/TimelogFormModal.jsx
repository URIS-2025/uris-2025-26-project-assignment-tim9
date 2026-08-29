import { useEffect, useMemo, useState } from 'react';
import Modal from './Modal';
import { useAuth } from '../auth/useAuth';
import { getWorkPackagesByProject } from '../api/workPackageApi';
import { getTasksByWorkPackage } from '../api/taskApi';
import { createTimelog, updateTimelog } from '../api/timelogApi';

function today() {
  return new Date().toISOString().slice(0, 10);
}

/**
 * @param {object} props
 * @param {Array<{projectId: string, name: string}>} props.projects - projects the user belongs to
 * @param {object|null} props.editingTimelog - pass an existing timelog to edit it, null to create
 * @param {() => void} props.onClose
 * @param {() => void} props.onSaved - called after a successful create/update
 */
export default function TimelogFormModal({ projects, editingTimelog, onClose, onSaved }) {
  const { token, userId } = useAuth();
  const isEditing = !!editingTimelog;

  const [projectId, setProjectId] = useState(editingTimelog?.projectId ?? '');
  const [taskId, setTaskId] = useState(editingTimelog?.taskId ?? '');
  const [hoursSpent, setHoursSpent] = useState(editingTimelog?.hoursSpent ?? '');
  const [date, setDate] = useState(
    editingTimelog?.date ? editingTimelog.date.slice(0, 10) : today()
  );

  const [tasks, setTasks] = useState([]);
  const [tasksLoading, setTasksLoading] = useState(false);
  const [tasksError, setTasksError] = useState(null);

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);

  const hasProjects = projects.length > 0;

  useEffect(() => {
    if (!projectId) return;
    let cancelled = false;
    (async () => {
      setTasksLoading(true);
      setTasksError(null);
      try {
        const workPackages = await getWorkPackagesByProject(projectId, token);
        const taskLists = await Promise.all(
          workPackages.map((wp) => getTasksByWorkPackage(wp.workPackageId, token))
        );
        if (cancelled) return;
        const flattened = taskLists.flat();
        setTasks(flattened);
        // If editing and the task belongs to this project's list, keep it selected;
        // otherwise clear a stale selection from a previous project.
        setTaskId((current) => (flattened.some((t) => t.taskId === current) ? current : ''));
      } catch (err) {
        if (!cancelled) setTasksError(err.message || 'Could not load tasks for this project.');
      } finally {
        if (!cancelled) setTasksLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [projectId, token]);

  const projectOptions = useMemo(
    () => [...projects].sort((a, b) => a.name.localeCompare(b.name)),
    [projects]
  );

  // Tasks belong to whichever project is currently selected - once the
  // project changes, stale tasks from the previous selection shouldn't
  // flash before the new fetch resolves.
  const visibleTasks = projectId ? tasks : [];

  async function handleSubmit(e) {
    e.preventDefault();
    setError(null);

    if (!projectId || !taskId) {
      setError('Pick a project and a task.');
      return;
    }

    const hours = Number(hoursSpent);
    if (!Number.isFinite(hours) || hours <= 0 || hours > 24) {
      setError('Hours must be greater than 0 and no more than 24.');
      return;
    }

    setSubmitting(true);
    try {
      const dto = { projectId, taskId, hoursSpent: hours, date };
      if (isEditing) {
        await updateTimelog(editingTimelog.id, dto, token, userId);
      } else {
        await createTimelog(dto, token, userId);
      }
      onSaved();
    } catch (err) {
      setError(err.message || 'Could not save the timelog.');
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <Modal title={isEditing ? 'Edit timelog' : 'Log time'} onClose={onClose}>
      {!hasProjects ? (
        <div className="form-message error">
          You are not a member of any project yet, so there is nothing to log time against.
        </div>
      ) : (
        <form className="stacked-form" onSubmit={handleSubmit}>
          {error && <div className="form-message error">{error}</div>}

          <label>
            Project
            <select
              required
              value={projectId}
              onChange={(e) => setProjectId(e.target.value)}
            >
              <option value="" disabled>
                Select a project…
              </option>
              {projectOptions.map((p) => (
                <option key={p.projectId} value={p.projectId}>
                  {p.name}
                </option>
              ))}
            </select>
          </label>

          <label>
            Task
            <select
              required
              value={taskId}
              disabled={!projectId || tasksLoading}
              onChange={(e) => setTaskId(e.target.value)}
            >
              <option value="" disabled>
                {tasksLoading
                  ? 'Loading tasks…'
                  : !projectId
                  ? 'Pick a project first'
                  : visibleTasks.length === 0
                  ? 'No tasks in this project'
                  : 'Select a task…'}
              </option>
              {visibleTasks.map((t) => (
                <option key={t.taskId} value={t.taskId}>
                  {t.title}
                </option>
              ))}
            </select>
            {tasksError && <span className="field-hint error">{tasksError}</span>}
          </label>

          <label>
            Date
            <input
              required
              type="date"
              max={today()}
              value={date}
              onChange={(e) => setDate(e.target.value)}
            />
          </label>

          <label>
            Hours spent
            <input
              required
              type="number"
              min="0"
              max="24"
              step="0.25"
              value={hoursSpent}
              onChange={(e) => setHoursSpent(e.target.value)}
            />
          </label>

          <div className="modal-actions">
            <button type="button" className="secondary-button" onClick={onClose}>
              Cancel
            </button>
            <button type="submit" className="primary-button" disabled={submitting}>
              {submitting ? 'Saving…' : isEditing ? 'Save changes' : 'Log time'}
            </button>
          </div>
        </form>
      )}
    </Modal>
  );
}
