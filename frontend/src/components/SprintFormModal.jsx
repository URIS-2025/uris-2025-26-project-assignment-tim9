import { useMemo, useState } from 'react';
import Modal from './Modal';
import { useAuth } from '../auth/useAuth';
import { createSprint } from '../api/sprintApi';
import { SPRINT_STATUSES } from '../shared/enums';

function today() {
  return new Date().toISOString().slice(0, 10);
}

/**
 * @param {object} props
 * @param {Array<{projectId: string, name: string}>} props.projects
 * @param {() => void} props.onClose
 * @param {(sprint: object) => void} props.onCreated - receives the created sprint (with .projectId)
 */
export default function SprintFormModal({ projects, onClose, onCreated }) {
  const { token } = useAuth();

  const [projectId, setProjectId] = useState('');
  const [name, setName] = useState('');
  const [status, setStatus] = useState('0');
  const [startDate, setStartDate] = useState(today());
  const [endDate, setEndDate] = useState(today());

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
      const created = await createSprint(
        projectId,
        { name: name.trim(), status: Number(status), startDate, endDate },
        token
      );
      onCreated(created);
    } catch (err) {
      setError(err.message || 'Could not create the sprint.');
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <Modal title="New sprint" onClose={onClose}>
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
              {submitting ? 'Creating…' : 'Create sprint'}
            </button>
          </div>
        </form>
      )}
    </Modal>
  );
}
