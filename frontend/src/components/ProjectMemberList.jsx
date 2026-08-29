import { useEffect, useMemo, useState } from 'react';
import { getProjectMembersByProjectId, deleteProjectMember } from '../api/projectApi';
import { sortBy } from '../utils/sortList';
import Modal from './Modal';
import ProjectMemberForm from './ProjectMemberForm';
import './ProjectMemberList.css';
import './rowActions.css';
import './listControls.css';

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

function ProjectMemberListSkeleton() {
  return (
    <ul className="member-list__items" aria-hidden="true">
      {Array.from({ length: 3 }).map((_, index) => (
        <li className="member-row member-row--skeleton" key={index}>
          <span className="pm-skeleton pm-skeleton--name" />
          <span className="pm-skeleton pm-skeleton--date" />
        </li>
      ))}
    </ul>
  );
}

export default function ProjectMemberList({
  projectId,
  token,
  reloadSignal = 0,
  canManage = false,
}) {
  const [members, setMembers] = useState([]);
  const [phase, setPhase] = useState('loading');
  const [reloadKey, setReloadKey] = useState(0);
  const [editing, setEditing] = useState(null);
  const [deletingItem, setDeletingItem] = useState(null);
  const [deleting, setDeleting] = useState(false);
  const [deleteError, setDeleteError] = useState('');
  const [roleFilter, setRoleFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [sort, setSort] = useState('');

  useEffect(() => {
    if (!projectId) return undefined;
    let ignore = false;

    getProjectMembersByProjectId(projectId, token)
      .then((data) => {
        if (ignore) return;
        setMembers(Array.isArray(data) ? data : []);
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

  const roleOptions = useMemo(
    () => [...new Set(members.map((m) => m.role).filter(Boolean))].sort(),
    [members]
  );

  const visible = useMemo(() => {
    let filtered = members;
    if (roleFilter) {
      filtered = filtered.filter((m) => m.role === roleFilter);
    }
    if (statusFilter === 'active') {
      filtered = filtered.filter((m) => m.status !== false);
    } else if (statusFilter === 'inactive') {
      filtered = filtered.filter((m) => m.status === false);
    }
    switch (sort) {
      case 'name-asc':
        return sortBy(filtered, (m) => m.username || m.userId || '', 'asc');
      case 'name-desc':
        return sortBy(filtered, (m) => m.username || m.userId || '', 'desc');
      case 'joined-desc':
        return sortBy(filtered, (m) => new Date(m.joinedAt), 'desc');
      case 'joined-asc':
        return sortBy(filtered, (m) => new Date(m.joinedAt), 'asc');
      default:
        return filtered;
    }
  }, [members, roleFilter, statusFilter, sort]);

  const confirmDelete = async () => {
    setDeleting(true);
    setDeleteError('');
    try {
      await deleteProjectMember(deletingItem.projectMemberId, token);
      setDeletingItem(null);
      setDeleting(false);
      reload();
    } catch (error) {
      const httpStatus = error && error.status;
      setDeleteError(
        httpStatus === 403
          ? "You don't have permission to remove members."
          : (error && error.message) ||
              'Something went wrong while removing the member. Please try again.'
      );
      setDeleting(false);
    }
  };

  return (
    <>
      {phase === 'loading' && <ProjectMemberListSkeleton />}

      {phase === 'error' && (
        <div className="member-list__state member-list__state--error" role="alert">
          <p>We couldn’t load the members.</p>
          <button type="button" className="member-list__retry" onClick={reload}>
            Try again
          </button>
        </div>
      )}

      {phase === 'ready' && members.length === 0 && (
        <p className="member-list__state member-list__state--empty">No members yet.</p>
      )}

      {phase === 'ready' && members.length > 0 && (
        <div className="list-toolbar">
          <label className="list-control">
            Role
            <select value={roleFilter} onChange={(event) => setRoleFilter(event.target.value)}>
              <option value="">All</option>
              {roleOptions.map((r) => (
                <option key={r} value={r}>
                  {r}
                </option>
              ))}
            </select>
          </label>
          <label className="list-control">
            Status
            <select value={statusFilter} onChange={(event) => setStatusFilter(event.target.value)}>
              <option value="">All</option>
              <option value="active">Active</option>
              <option value="inactive">Inactive</option>
            </select>
          </label>
          <label className="list-control">
            Sort
            <select value={sort} onChange={(event) => setSort(event.target.value)}>
              <option value="">Default</option>
              <option value="name-asc">Name (A–Z)</option>
              <option value="name-desc">Name (Z–A)</option>
              <option value="joined-desc">Joined (newest)</option>
              <option value="joined-asc">Joined (oldest)</option>
            </select>
          </label>
        </div>
      )}

      {phase === 'ready' && members.length > 0 && visible.length === 0 && (
        <p className="member-list__state member-list__state--empty">
          No members match the filters.
        </p>
      )}

      {phase === 'ready' && visible.length > 0 && (
        <ul className="member-list__items">
          {visible.map((member, index) => {
            const joined = formatDate(member.joinedAt);
            return (
              <li className="member-row" key={member.projectMemberId ?? index}>
                <span className="member-row__id">
                  <span className="member-row__name">{member.username || member.userId}</span>
                  <span className="member-row__role">{member.role || 'Unknown role'}</span>
                </span>
                <span className="member-row__right">
                  <span className="member-row__joined">
                    {joined ? `Joined ${joined}` : 'Join date unknown'}
                  </span>
                  {canManage && (
                    <span className="row-actions">
                      <button
                        type="button"
                        className="row-action"
                        onClick={() => setEditing(member)}
                      >
                        Edit
                      </button>
                      <button
                        type="button"
                        className="row-action row-action--danger"
                        onClick={() => {
                          setDeleteError('');
                          setDeletingItem(member);
                        }}
                      >
                        Remove
                      </button>
                    </span>
                  )}
                </span>
              </li>
            );
          })}
        </ul>
      )}

      {editing && (
        <Modal title="Edit Member" onClose={() => setEditing(null)}>
          <ProjectMemberForm
            mode="edit"
            member={editing}
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
          title="Remove Member"
          onClose={() => {
            if (!deleting) setDeletingItem(null);
          }}
        >
          <p className="row-confirm-text">
            Are you sure you want to remove this member from the project?
          </p>

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
              {deleting ? 'Removing…' : 'Remove Member'}
            </button>
          </div>
        </Modal>
      )}
    </>
  );
}
