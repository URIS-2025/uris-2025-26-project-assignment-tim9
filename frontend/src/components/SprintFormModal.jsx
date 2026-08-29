import { useMemo, useState } from 'react';
import Modal from './Modal';
import { useAuth } from '../auth/useAuth';
import { createSprint, updateSprint } from '../api/sprintApi';
import { SPRINT_STATUSES } from '../shared/enums';

function today() {
  return new Date().toISOString().slice(0, 10);
}

// <input type="date"> wants YYYY-MM-DD; the backend sends a full ISO
// DateTime (e.g. "2026-08-27T00:00:00").
function toDateInputValue(iso) {
  return iso ? iso.slice(0, 10) : today();
}

/**
 * Doubles as the create and edit form - pass `sprint` to edit it in place
 * (fields prefilled, PUT on submit) or omit it to create a new one (POST).
 *
 * @param {object} props
 * @param {Array<{projectId: string, name: string}>} props.projects
 * @param {object} [props.sprint] - the sprint being edited; omit to create instead
 * @param {string} [props.initialProjectId] - preselect this project (e.g. when opened from a
 *   project-scoped sprints page); ignored while editing, and still changeable by the user
 * @param {() => void} props.onClose
 * @param {(sprint: object) => void} props.onSaved - receives the created/updated sprint (with .projectId)
 */
export default function SprintFormModal({ projects, sprint, initialProjectId, onClose, onSaved }) {
  const { token } = useAuth();
  const isEditing = Boolean(sprint);

  const [projectId, setProjectId] = useState(sprint?.projectId ?? initialProjectId ?? '');
  const [name, setName] = useState(sprint?.name ?? '');
  const [status, setStatus] = useState(String(sprint?.status ?? 0));
  const [startDate, setStartDate] = useState(toDateInputValue(sprint?.startDate));
  const [endDate, setEndDate] = useState(toDateInputValue(sprint?.endDate));

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);

  const projectOptions = useMemo(
    () => [...projects].sort((a, b) => a.name.localeCompare(b.name)),
    [projects]
  );
  const hasProjects = projects.length > 0;

  async function handleSubmit(e) {
    e.preventDefault();
    setError(null);

    if (!projectId) {
      setError('Pick a project to attach this sprint to.');
      return;
    }
    if (name.trim().length < 3) {
      setError('Sprint name must be at least 3 characters.');
      return;
    }
    if (new Date(endDate) <= new Date(startDate)) {
      setError('End date must be after the start date.');
      return;
    }

    setSubmitting(true);
    try {
      const payload = { name: name.trim(), status: Number(status), startDate, endDate };
      const saved = isEditing
        ? await updateSprint(sprint.id, { projectId, ...payload }, token)
        : await createSprint(projectId, payload, token);
      onSaved(saved);
    } catch (err) {
      setError(err.message || `Could not ${isEditing ? 'save' : 'create'} the sprint.`);
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <Modal title={isEditing ? 'Edit sprint' : 'New sprint'} onClose={onClose}>
      {!hasProjects ? (
        <div className="form-message error">
          There are no projects to attach a sprint to yet.
        </div>
      ) : (
        <form className="stacked-form" onSubmit={handleSubmit}>
          {error && <div className="form-message error">{error}</div>}

          <label>
            Project
            <select required value={projectId} onChange={(e) => setProjectId(e.target.value)}>
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
            Sprint name
            <input required minLength={3} value={name} onChange={(e) => setName(e.target.value)} />
          </label>

          <label>
            Status
            <select value={status} onChange={(e) => setStatus(e.target.value)}>
              {SPRINT_STATUSES.map((label, index) => (
                <option key={label} value={index}>
                  {label}
                </option>
              ))}
            </select>
          </label>

          <label>
            Start date
            <input
              required
              type="date"
              value={startDate}
              onChange={(e) => setStartDate(e.target.value)}
            />
          </label>

          <label>
            End date
            <input
              required
              type="date"
              min={startDate}
              value={endDate}
              onChange={(e) => setEndDate(e.target.value)}
            />
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
                  : 'Create sprint'}
            </button>
          </div>
        </form>
      )}
    </Modal>
  );
}
