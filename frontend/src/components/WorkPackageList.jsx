import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
//import { getWorkPackages } from '../api/workPackageApi';
import './WorkPackageList.css';

const MOCK_WORK_PACKAGES = [
  { id: '1', name: 'Authentication and authorization', description: 'JWT middleware and role-based auth', status: 'InProgress' },
  { id: '2', name: 'Task management', description: 'CRUD for the Task entity with sub-tasks', status: 'Done' },
  { id: '3', name: 'Notifications', description: 'Integration with NotificationService', status: 'ToDo' },
];

const USE_MOCK_DATA = true; // promeni u false kad backend proradi

const STATUS_META = {
  ToDo: { label: 'To Do', background: 'var(--code-bg)', color: 'var(--text)' },
  InProgress: { label: 'In Progress', background: 'var(--color-status-in-progress)', color: '#fff' },
  Done: { label: 'Done', background: 'var(--color-status-done)', color: '#fff' },
  Critical: { label: 'Critical', background: 'var(--color-status-critical)', color: '#fff' },
};

const STATUS_OPTIONS = [
  { value: 'ToDo', label: 'To Do' },
  { value: 'InProgress', label: 'In Progress' },
  { value: 'Done', label: 'Done' },
];

export default function WorkPackageList({ projectId, onCreateClick }) {
  const [workPackages, setWorkPackages] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    if (USE_MOCK_DATA) {
      setWorkPackages(MOCK_WORK_PACKAGES);
      setLoading(false);
      return;
    }

    getWorkPackages(projectId)
      .then(setWorkPackages)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, [projectId]);

  function openWorkPackage(id) {
    navigate(`/projects/${projectId}/work-packages/${id}`);
  }

  function handleDelete(event, id) {
    event.stopPropagation();
    if (!window.confirm('Delete this work package?')) return;
    setWorkPackages((prev) => prev.filter((wp) => wp.id !== id));
  }

  function handleStatusChange(id, status) {
    setWorkPackages((prev) => prev.map((wp) => (wp.id === id ? { ...wp, status } : wp)));
  }

  if (loading) return <p>Loading...</p>;
  if (error) return <p className="wp-list__error">{error}</p>;

  return (
    <div className="wp-list">
      <div className="wp-grid">
        {workPackages.map((wp) => {
          const meta = STATUS_META[wp.status] ?? {
            label: wp.status,
            background: 'var(--code-bg)',
            color: 'var(--text)',
          };

          return (
            <article
              key={wp.id}
              className="wp-card"
              role="button"
              tabIndex={0}
              onClick={() => openWorkPackage(wp.id)}
              onKeyDown={(event) => {
                if (event.key === 'Enter' || event.key === ' ') {
                  event.preventDefault();
                  openWorkPackage(wp.id);
                }
              }}
            >
              <button
                type="button"
                className="wp-card__delete"
                aria-label="Delete work package"
                onClick={(event) => handleDelete(event, wp.id)}
              >
                ×
              </button>

              <select
                className="wp-card__badge"
                style={{ background: meta.background, color: meta.color }}
                value={wp.status}
                aria-label="Work package status"
                onClick={(event) => event.stopPropagation()}
                onChange={(event) => handleStatusChange(wp.id, event.target.value)}
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
