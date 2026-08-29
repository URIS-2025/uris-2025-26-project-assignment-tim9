import { useEffect, useMemo, useState } from 'react';
import { getRequirementsByProjectId, deleteRequirement } from '../api/projectApi';
import { sortBy } from '../utils/sortList';
import Modal from './Modal';
import RequirementForm from './RequirementForm';
import './RequirementList.css';
import './rowActions.css';
import './listControls.css';

function RequirementListSkeleton() {
  return (
    <ul className="requirement-list__items" aria-hidden="true">
      {Array.from({ length: 3 }).map((_, index) => (
        <li className="requirement-row requirement-row--skeleton" key={index}>
          <span className="rl-skeleton rl-skeleton--line" />
          <span className="rl-skeleton rl-skeleton--line is-short" />
        </li>
      ))}
    </ul>
  );
}

export default function RequirementList({
  projectId,
  token,
  reloadSignal = 0,
  canManage = false,
}) {
  const [requirements, setRequirements] = useState([]);
  const [phase, setPhase] = useState('loading');
  const [reloadKey, setReloadKey] = useState(0);
  const [editing, setEditing] = useState(null);
  const [deletingItem, setDeletingItem] = useState(null);
  const [deleting, setDeleting] = useState(false);
  const [deleteError, setDeleteError] = useState('');
  const [filterText, setFilterText] = useState('');
  const [sortDir, setSortDir] = useState('');

  useEffect(() => {
    if (!projectId) return undefined;
    let ignore = false;

    getRequirementsByProjectId(projectId, token)
      .then((data) => {
        if (ignore) return;
        setRequirements(Array.isArray(data) ? data : []);
        setPhase('ready');
      })
      .catch(() => {
        if (ignore) return;
        setPhase('error');
      });

    return () => {
      ignore = true;
    };
  }, [projectId, token, reloadKey, reloadSignal]);

  const reload = () => {
    setPhase('loading');
    setReloadKey((key) => key + 1);
  };

  const visible = useMemo(() => {
    const needle = filterText.trim().toLowerCase();
    const filtered = needle
      ? requirements.filter((r) => (r.description || '').toLowerCase().includes(needle))
      : requirements;
    if (sortDir === 'asc' || sortDir === 'desc') {
      return sortBy(filtered, (r) => r.description || '', sortDir);
    }
    return filtered;
  }, [requirements, filterText, sortDir]);

  const confirmDelete = async () => {
    setDeleting(true);
    setDeleteError('');
    try {
      await deleteRequirement(deletingItem.requirementId, token);
      setDeletingItem(null);
      setDeleting(false);
      reload();
    } catch (error) {
      const httpStatus = error && error.status;
      setDeleteError(
        httpStatus === 403
          ? "You don't have permission to delete requirements."
          : (error && error.message) ||
              'Something went wrong while deleting the requirement. Please try again.'
      );
      setDeleting(false);
    }
  };

  return (
    <>
      {phase === 'loading' && <RequirementListSkeleton />}

      {phase === 'error' && (
        <div className="requirement-list__state requirement-list__state--error" role="alert">
          <p>We couldn’t load the requirements.</p>
          <button type="button" className="requirement-list__retry" onClick={reload}>
            Try again
          </button>
        </div>
      )}

      {phase === 'ready' && requirements.length === 0 && (
        <p className="requirement-list__state requirement-list__state--empty">
          No requirements yet.
        </p>
      )}

      {phase === 'ready' && requirements.length > 0 && (
        <div className="list-toolbar">
          <label className="list-control">
            Filter
            <input
              type="text"
              value={filterText}
              onChange={(event) => setFilterText(event.target.value)}
              placeholder="Search descriptions"
            />
          </label>
          <label className="list-control">
            Sort
            <select value={sortDir} onChange={(event) => setSortDir(event.target.value)}>
              <option value="">Default</option>
              <option value="asc">Description (A–Z)</option>
              <option value="desc">Description (Z–A)</option>
            </select>
          </label>
        </div>
      )}

      {phase === 'ready' && requirements.length > 0 && visible.length === 0 && (
        <p className="requirement-list__state requirement-list__state--empty">
          No requirements match the filter.
        </p>
      )}

      {phase === 'ready' && visible.length > 0 && (
        <ul className="requirement-list__items">
          {visible.map((requirement, index) => (
            <li className="requirement-row" key={requirement.requirementId ?? index}>
              <span className="requirement-row__text">
                {requirement.description || 'No description'}
              </span>
              {canManage && (
                <span className="row-actions">
                  <button
                    type="button"
                    className="row-action"
                    onClick={() => setEditing(requirement)}
                  >
                    Edit
                  </button>
                  <button
                    type="button"
                    className="row-action row-action--danger"
                    onClick={() => {
                      setDeleteError('');
                      setDeletingItem(requirement);
                    }}
                  >
                    Delete
                  </button>
                </span>
              )}
            </li>
          ))}
        </ul>
      )}

      {editing && (
        <Modal title="Edit Requirement" onClose={() => setEditing(null)}>
          <RequirementForm
            mode="edit"
            requirement={editing}
            projectId={projectId}
            token={token}
            onCancel={() => setEditing(null)}
            onSaved={() => {
              setEditing(null);
              reload();
            }}
          />
        </Modal>
      )}

      {deletingItem && (
        <Modal
          title="Delete Requirement"
          onClose={() => {
            if (!deleting) setDeletingItem(null);
          }}
        >
          <p className="row-confirm-text">Are you sure you want to delete this requirement?</p>

          {deleteError && (
            <p className="row-delete-error" role="alert">
              {deleteError}
            </p>
          )}

          <div className="modal-actions">
            <button
              type="button"
              className="secondary-button"
              onClick={() => setDeletingItem(null)}
              disabled={deleting}
            >
              Cancel
            </button>
            <button
              type="button"
              className="row-delete-confirm"
              onClick={confirmDelete}
              disabled={deleting}
            >
              {deleting ? 'Deleting…' : 'Delete Requirement'}
            </button>
          </div>
        </Modal>
      )}
    </>
  );
}
