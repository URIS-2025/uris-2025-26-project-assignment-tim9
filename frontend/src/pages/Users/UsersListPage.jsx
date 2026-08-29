import { useEffect, useState } from 'react';
import { useAuth } from '../../auth/useAuth';
import { getUsers, deactivateUser, activateUser } from '../../api/userApi';
import Modal from '../../components/Modal';
import RoleChangeForm from '../../components/RoleChangeForm';
import UserActivityModal from '../../components/UserActivityModal';
import '../../components/listControls.css';
import '../../components/rowActions.css';
import './UsersListPage.css';

const ROLES = ['Admin', 'ProjectManager', 'TeamMember', 'Client'];

export default function UsersListPage() {
  const { token, userId: currentAdminId } = useAuth();

  const [users, setUsers] = useState([]);
  const [phase, setPhase] = useState('loading');
  const [errorMessage, setErrorMessage] = useState('');
  const [reloadKey, setReloadKey] = useState(0);

  const [search, setSearch] = useState('');
  const [roleFilter, setRoleFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');

  const [roleModalUser, setRoleModalUser] = useState(null);
  const [activityModalUser, setActivityModalUser] = useState(null);
  const [pendingUserId, setPendingUserId] = useState(null);
  const [rowError, setRowError] = useState('');

  useEffect(() => {
    let ignore = false;

    const timer = setTimeout(() => {
      if (ignore) return;
      setPhase('loading');
      setErrorMessage('');

      getUsers(token, {
        search: search || undefined,
        role: roleFilter || undefined,
        isActive: statusFilter || undefined,
      })
        .then((data) => {
          if (ignore) return;
          setUsers(data);
          setPhase('ready');
        })
        .catch((error) => {
          if (ignore) return;
          setErrorMessage(
            error && error.status === 403
              ? "You don't have permission to view the user list."
              : 'Something went wrong while loading users. Check your connection and try again.'
          );
          setPhase('error');
        });
    }, 250);

    return () => {
      ignore = true;
      clearTimeout(timer);
    };
  }, [token, search, roleFilter, statusFilter, reloadKey]);

  const reload = () => setReloadKey((key) => key + 1);

  const toggleActive = async (user) => {
    setRowError('');
    setPendingUserId(user.userId);
    try {
      if (user.isActive) {
        await deactivateUser(user.userId, currentAdminId, token);
      } else {
        await activateUser(user.userId, currentAdminId, token);
      }
      reload();
    } catch (error) {
      setRowError(
        error && error.status === 403
          ? "You don't have permission to change this user's status."
          : "Something went wrong. The user's status was not changed."
      );
    } finally {
      setPendingUserId(null);
    }
  };

  return (
    <section className="users-page">
      <header className="users-header">
        <h1 className="users-title">Users</h1>
        {phase === 'ready' && <span className="users-count">{users.length}</span>}
        <p className="users-subtitle">Manage accounts, roles and access.</p>
      </header>

      <div className="list-toolbar">
        <label className="list-control">
          Search
          <input
            type="text"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Name, username or email"
          />
        </label>

        <label className="list-control">
          Role
          <select value={roleFilter} onChange={(event) => setRoleFilter(event.target.value)}>
            <option value="">All</option>
            {ROLES.map((r) => (
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
            <option value="true">Active</option>
            <option value="false">Inactive</option>
          </select>
        </label>
      </div>

      {rowError && (
        <p className="form-message error" role="alert">
          {rowError}
        </p>
      )}

      {phase === 'loading' && <p className="status-hint">Loading users…</p>}

      {phase === 'error' && (
        <div className="users-state users-state--error" role="alert">
          <p>{errorMessage}</p>
          <button type="button" className="secondary-button" onClick={reload}>
            Try again
          </button>
        </div>
      )}

      {phase === 'ready' && users.length === 0 && (
        <div className="users-state users-state--empty">
          <p>No users match the current filters.</p>
        </div>
      )}

      {phase === 'ready' && users.length > 0 && (
        <div className="users-list">
          <div className="users-row users-row-head">
            <span>Name</span>
            <span>Username</span>
            <span>Email</span>
            <span>Role</span>
            <span>Status</span>
            <span className="align-right">Actions</span>
          </div>
          {users.map((user) => (
            <div className="users-row" key={user.userId}>
              <span>{user.name}</span>
              <span className="users-username">{user.username}</span>
              <span className="users-email" title={user.email}>
                {user.email}
              </span>
              <span>
                <span className="role-badge">{user.role}</span>
              </span>
              <span>
                <span
                  className={
                    user.isActive ? 'status-badge status-badge--active' : 'status-badge'
                  }
                >
                  {user.isActive ? 'Active' : 'Inactive'}
                </span>
              </span>
              <span className="row-actions align-right">
                <button
                  type="button"
                  className="row-action"
                  onClick={() => setRoleModalUser(user)}
                >
                  Change role
                </button>
                <button
                  type="button"
                  className="row-action"
                  disabled={pendingUserId === user.userId}
                  onClick={() => toggleActive(user)}
                >
                  {pendingUserId === user.userId
                    ? 'Working…'
                    : user.isActive
                      ? 'Deactivate'
                      : 'Activate'}
                </button>
                <button
                  type="button"
                  className="row-action"
                  onClick={() => setActivityModalUser(user)}
                >
                  Activity
                </button>
              </span>
            </div>
          ))}
        </div>
      )}

      {roleModalUser && (
        <Modal title={`Change role — ${roleModalUser.username}`} onClose={() => setRoleModalUser(null)}>
          <RoleChangeForm
            user={roleModalUser}
            currentAdminId={currentAdminId}
            token={token}
            onCancel={() => setRoleModalUser(null)}
            onSaved={() => {
              setRoleModalUser(null);
              reload();
            }}
          />
        </Modal>
      )}

      {activityModalUser && (
        <Modal
          title={`Activity — ${activityModalUser.username}`}
          onClose={() => setActivityModalUser(null)}
        >
          <UserActivityModal userId={activityModalUser.userId} token={token} />
        </Modal>
      )}
    </section>
  );
}
