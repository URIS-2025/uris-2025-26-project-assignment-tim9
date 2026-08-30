import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/useAuth';
import { getFriendlyErrorMessage } from '../utils/errorMessages';
import { useToast } from '../shared/components/useToast';
import {
  getWorkPackages,
  updateWorkPackage,
  updateWorkPackageStatus,
  deleteWorkPackage,
  WORK_PACKAGE_STATUS_LABELS,
} from '../api/workPackageApi';
import './WorkPackageList.css';

// WorkPackageStatus enum (backend, integer on the wire):
// 0 Planned, 1 InProgress, 2 OnHold, 3 Completed, 4 Cancelled
const STATUS_META = {
  0: { label: 'Planned', background: 'var(--code-bg)', color: 'var(--text)' },
  1: { label: 'In Progress', background: 'var(--color-status-in-progress)', color: '#fff' },
  2: { label: 'On Hold', background: 'var(--code-bg)', color: 'var(--text)' },
  3: { label: 'Completed', background: 'var(--color-status-done)', color: '#fff' },
  4: { label: 'Cancelled', background: 'var(--color-status-critical)', color: '#fff' },
};

const STATUS_OPTIONS = [
  { value: 0, label: 'Planned' },
  { value: 1, label: 'In Progress' },
  { value: 2, label: 'On Hold' },
  { value: 3, label: 'Completed' },
  { value: 4, label: 'Cancelled' },
];

function toDateInput(value) {
  if (!value) return '';
  const d = new Date(value);
  return Number.isNaN(d.getTime()) ? '' : d.toISOString().slice(0, 10);
}

export default function WorkPackageList({ projectId, onCreateClick }) {
  const { token, role } = useAuth();
  const { showToast } = useToast();
  const canManage = role === 'Admin' || role === 'ProjectManager';
  const [workPackages, setWorkPackages] = useState([]);
  const [phase, setPhase] = useState('loading');
  const [errorMessage, setErrorMessage] = useState('');
  const [reloadKey, setReloadKey] = useState(0);
  const navigate = useNavigate();

  const [editingId, setEditingId] = useState(null);
  const [editForm, setEditForm] = useState({ name: '', description: '', deadline: '' });
  const [savingEdit, setSavingEdit] = useState(false);
  const [editError, setEditError] = useState('');

  useEffect(() => {
    let ignore = false;

    getWorkPackages(projectId, token)
      .then((data) => {
        if (ignore) return;
        setWorkPackages(Array.isArray(data) ? data : []);
        setErrorMessage('');
        setPhase('ready');
      })
      .catch((error) => {
        if (ignore) return;
        setErrorMessage(
          error && error.status === 401
            ? 'Your session has expired. Please sign in again.'
            : 'Something went wrong while loading the work packages.',
        );
        setPhase('error');
      });

    return () => {
      ignore = true;
    };
  }, [projectId, token, reloadKey]);

  const reload = () => {
    setPhase('loading');
    setReloadKey((key) => key + 1);
  };

  function openWorkPackage(id) {
    navigate(`/projects/${projectId}/work-packages/${id}`);
  }

  function startEdit(event, wp) {
    event.stopPropagation();
    setEditingId(wp.workPackageId);
    setEditForm({
      name: wp.name ?? '',
      description: wp.description ?? '',
      deadline: toDateInput(wp.deadline),
    });
    setEditError('');
  }

  function cancelEdit(event) {
    event?.stopPropagation();
    setEditingId(null);
    setEditError('');
  }

  async function saveEdit(event) {
    event.preventDefault();
    if (!editForm.name.trim()) return;
    setSavingEdit(true);
    setEditError('');
    try {
      const updated = await updateWorkPackage(
        editingId,
        {
          name: editForm.name.trim(),
          description: editForm.description,
          deadline: editForm.deadline,
        },
        token,
      );
      setWorkPackages((prev) =>
        prev.map((wp) =>
          wp.workPackageId === editingId
            ? {
                ...wp,
                name: editForm.name.trim(),
                description: editForm.description,
                deadline: updated?.deadline ?? editForm.deadline,
              }
            : wp,
        ),
      );
      setEditingId(null);
    } catch (error) {
      setEditError(getFriendlyErrorMessage(error, 'work-package-write'));
    } finally {
      setSavingEdit(false);
    }
  }

  async function handleDelete(event, id) {
    event.stopPropagation();
    if (!window.confirm('Delete this work package?')) return;
    try {
      await deleteWorkPackage(id, token);
      setWorkPackages((prev) => prev.filter((wp) => wp.workPackageId !== id));
      showToast('Work package deleted.', 'success');
    } catch (error) {
      showToast(getFriendlyErrorMessage(error, 'work-package-write'), 'error');
    }
  }

  async function handleStatusChange(id, status) {
    const nextStatus = Number(status);
    const previous = workPackages;
    setWorkPackages((prev) =>
      prev.map((wp) => (wp.workPackageId === id ? { ...wp, status: nextStatus } : wp)),
    );
    try {
      await updateWorkPackageStatus(id, nextStatus, token);
    } catch (error) {
      setWorkPackages(previous);
      showToast(getFriendlyErrorMessage(error, 'work-package-write'), 'error');
    }
  }

  if (phase === 'loading') return <p>Loading...</p>;
  if (phase === 'error') {
    return (
      <p className="wp-list__error">
        {errorMessage}{' '}
        <button type="button" onClick={reload}>
          Retry
        </button>
      </p>
    );
  }

  return (
    <div className="wp-list">
      <div className="wp-grid">
        {workPackages.map((wp) => {
          const meta = STATUS_META[wp.status] ?? {
            label: WORK_PACKAGE_STATUS_LABELS[wp.status] ?? String(wp.status),
            background: 'var(--code-bg)',
            color: 'var(--text)',
          };
          const isEditing = editingId === wp.workPackageId;

          if (isEditing) {
            return (
              <form
                key={wp.workPackageId}
                className="wp-card wp-card--editing"
                onClick={(event) => event.stopPropagation()}
                onSubmit={saveEdit}
              >
                <label className="wp-card__field">
                  Name
                  <input
                    type="text"
                    value={editForm.name}
                    onChange={(e) => setEditForm((f) => ({ ...f, name: e.target.value }))}
                    required
                  />
                </label>
                <label className="wp-card__field">
                  Description
                  <textarea
                    rows={2}
                    value={editForm.description}
                    onChange={(e) => setEditForm((f) => ({ ...f, description: e.target.value }))}
                  />
                </label>
                <label className="wp-card__field">
                  Deadline
                  <input
                    type="date"
                    value={editForm.deadline}
                    onChange={(e) => setEditForm((f) => ({ ...f, deadline: e.target.value }))}
                  />
                </label>
                {editError && <p className="wp-list__error">{editError}</p>}
                <div className="wp-card__edit-actions">
                  <button type="submit" disabled={savingEdit}>
                    {savingEdit ? 'Saving...' : 'Save'}
                  </button>
                  <button type="button" onClick={cancelEdit} disabled={savingEdit}>
                    Cancel
                  </button>
                </div>
              </form>
            );
          }

          return (
            <article
              key={wp.workPackageId}
              className="wp-card"
              role="button"
              tabIndex={0}
              onClick={() => openWorkPackage(wp.workPackageId)}
              onKeyDown={(event) => {
                if (event.key === 'Enter' || event.key === ' ') {
                  event.preventDefault();
                  openWorkPackage(wp.workPackageId);
                }
              }}
            >
              {canManage && (
                <div className="wp-card__actions">
                  <button
                    type="button"
                    className="wp-card__edit"
                    aria-label="Edit work package"
                    onClick={(event) => startEdit(event, wp)}
                  >
                    ✎
                  </button>
                  <button
                    type="button"
                    className="wp-card__delete"
                    aria-label="Delete work package"
                    onClick={(event) => handleDelete(event, wp.workPackageId)}
                  >
                    ×
                  </button>
                </div>
              )}

              <select
                className="wp-card__badge"
                style={{ background: meta.background, color: meta.color }}
                value={wp.status}
                aria-label="Work package status"
                disabled={!canManage}
                onClick={(event) => event.stopPropagation()}
                onChange={(event) => handleStatusChange(wp.workPackageId, event.target.value)}
              >
                {STATUS_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>

              <h3 className="wp-card__title">{wp.name}</h3>
              <p className="wp-card__desc">{wp.description}</p>
            </article>
          );
        })}

        <button type="button" className="wp-card wp-card--add" onClick={onCreateClick}>
          + New Work Package
        </button>
      </div>
    </div>
  );
}
