import { useEffect, useMemo, useState } from 'react';
import { getMilestonesByProjectId, deleteMilestone } from '../api/projectApi';
import { sortBy } from '../utils/sortList';
import Modal from './Modal';
import MilestoneForm from './MilestoneForm';
import './MilestoneList.css';
import './rowActions.css';
import './listControls.css';

const STATE_FILTERS = ['', 'Upcoming', 'Overdue'];

const dateFormat = new Intl.DateTimeFormat(undefined, {
  day: 'numeric',
  month: 'short',
  year: 'numeric',
});

function formatDate(value) {
  if (!value) return null;
  const parsed = new Date(value);
  return Number.isNaN(parsed.getTime()) ? null : dateFormat.format(parsed);
}

function resolveMilestoneState(expectedDate) {
  const parsed = new Date(expectedDate);
  if (Number.isNaN(parsed.getTime())) return { label: 'Unknown', tone: 'neutral' };
  return parsed.getTime() < Date.now()
    ? { label: 'Overdue', tone: 'critical' }
    : { label: 'Upcoming', tone: 'accent' };
}

function MilestoneListSkeleton() {
  return (
    <ul className="milestone-list__items" aria-hidden="true">
      {Array.from({ length: 3 }).map((_, index) => (
        <li className="milestone-row milestone-row--skeleton" key={index}>
          <div className="milestone-row__main">
            <span className="ml-skeleton ml-skeleton--name" />
            <span className="ml-skeleton ml-skeleton--date" />
          </div>
        </li>
      ))}
    </ul>
  );
}

export default function MilestoneList({ projectId, token, reloadSignal = 0, canManage = false }) {
  const [milestones, setMilestones] = useState([]);
  const [phase, setPhase] = useState('loading');
  const [reloadKey, setReloadKey] = useState(0);
  const [editing, setEditing] = useState(null);
  const [deletingItem, setDeletingItem] = useState(null);
  const [deleting, setDeleting] = useState(false);
  const [deleteError, setDeleteError] = useState('');
  const [stateFilter, setStateFilter] = useState('');
  const [sortDir, setSortDir] = useState('asc');

  useEffect(() => {
    if (!projectId) return undefined;
    let ignore = false;

    getMilestonesByProjectId(projectId, token)
      .then((data) => {
        if (ignore) return;
        setMilestones(Array.isArray(data) ? data : []);
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
    const filtered = stateFilter
      ? milestones.filter((m) => resolveMilestoneState(m.expectedDate).label === stateFilter)
      : milestones;
    return sortBy(filtered, (m) => new Date(m.expectedDate), sortDir);
  }, [milestones, stateFilter, sortDir]);

  const confirmDelete = async () => {
    setDeleting(true);
    setDeleteError('');
    try {
      await deleteMilestone(deletingItem.milestoneId, token);
      setDeletingItem(null);
      setDeleting(false);
      reload();
    } catch (error) {
      const httpStatus = error && error.status;
      setDeleteError(
        httpStatus === 403
          ? "You don't have permission to delete milestones."
          : (error && error.message) ||
              'Something went wrong while deleting the milestone. Please try again.'
      );
      setDeleting(false);
    }
  };

  return (
    <>
      {phase === 'loading' && <MilestoneListSkeleton />}

      {phase === 'error' && (
        <div className="milestone-list__state milestone-list__state--error" role="alert">
          <p>We couldn’t load the milestones.</p>
          <button type="button" className="milestone-list__retry" onClick={reload}>
            Try again
          </button>
        </div>
      )}

      {phase === 'ready' && milestones.length === 0 && (
        <p className="milestone-list__state milestone-list__state--empty">No milestones yet.</p>
      )}

      {phase === 'ready' && milestones.length > 0 && (
        <div className="list-toolbar">
          <span className="list-chips">
            {STATE_FILTERS.map((filter) => (
              <button
                key={filter || 'all'}
                type="button"
                className={stateFilter === filter ? 'list-chip is-active' : 'list-chip'}
                onClick={() => setStateFilter(filter)}
              >
                {filter || 'All'}
              </button>
            ))}
          </span>
          <label className="list-control">
            Sort
            <select value={sortDir} onChange={(event) => setSortDir(event.target.value)}>
              <option value="asc">Date (soonest)</option>
              <option value="desc">Date (latest)</option>
            </select>
          </label>
        </div>
      )}

      {phase === 'ready' && milestones.length > 0 && visible.length === 0 && (
        <p className="milestone-list__state milestone-list__state--empty">
          No milestones match the filter.
        </p>
      )}

      {phase === 'ready' && visible.length > 0 && (
        <ol className="milestone-list__items">
          {visible.map((milestone, index) => {
            const state = resolveMilestoneState(milestone.expectedDate);
            const date = formatDate(milestone.expectedDate);
            return (
              <li className="milestone-row" key={milestone.milestoneId ?? index}>
                <div className="milestone-row__main">
                  <span className="milestone-row__name">
                    {milestone.name || 'Untitled milestone'}
                  </span>
                  {milestone.description && (
                    <span className="milestone-row__description">{milestone.description}</span>
                  )}
                  <span className="milestone-row__meta">
                    <span className="milestone-row__date">{date ?? 'Unknown date'}</span>
                    <span className={`milestone-pill milestone-pill--${state.tone}`}>
                      {state.label}
                    </span>
                  </span>
                </div>
                {canManage && (
                  <span className="row-actions">
                    <button
                      type="button"
                      className="row-action"
                      onClick={() => setEditing(milestone)}
                    >
                      Edit
                    </button>
                    <button
                      type="button"
                      className="row-action row-action--danger"
                      onClick={() => {
                        setDeleteError('');
                        setDeletingItem(milestone);
                      }}
                    >
                      Delete
                    </button>
                  </span>
                )}
              </li>
            );
          })}
        </ol>
      )}

      {editing && (
        <Modal title="Edit Milestone" onClose={() => setEditing(null)}>
          <MilestoneForm
            mode="edit"
            milestone={editing}
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
          title="Delete Milestone"
          onClose={() => {
            if (!deleting) setDeletingItem(null);
          }}
        >
          <p className="row-confirm-text">Are you sure you want to delete this milestone?</p>

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
              {deleting ? 'Deleting…' : 'Delete Milestone'}
            </button>
          </div>
        </Modal>
      )}
    </>
  );
}
