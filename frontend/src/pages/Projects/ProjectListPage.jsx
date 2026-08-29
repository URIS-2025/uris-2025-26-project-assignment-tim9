import { useEffect, useMemo, useState } from 'react';
import { useAuth } from '../../auth/useAuth';
import { getProjects, getProjectsByUserId } from '../../api/projectApi';
import { STATUS_ORDER, STATUS_META, resolveStatus } from '../../utils/projectStatus';
import { sortBy } from '../../utils/sortList';
import ProjectCard from '../../components/ProjectCard';
import ProjectListSkeleton from '../../components/ProjectListSkeleton';
import ProjectsState from '../../components/ProjectsState';
import Modal from '../../components/Modal';
import ProjectForm from '../../components/ProjectForm';
import './ProjectListPage.css';

const SORT_OPTIONS = [
  { value: '', label: 'Default' },
  { value: 'name-asc', label: 'Name (A–Z)' },
  { value: 'name-desc', label: 'Name (Z–A)' },
  { value: 'budget-desc', label: 'Budget (high–low)' },
  { value: 'budget-asc', label: 'Budget (low–high)' },
  { value: 'deadline-asc', label: 'Deadline (soonest)' },
  { value: 'deadline-desc', label: 'Deadline (latest)' },
  { value: 'created-desc', label: 'Created (newest)' },
  { value: 'created-asc', label: 'Created (oldest)' },
  { value: 'status-asc', label: 'Status' },
];

const PAGE_SIZE = 9;

function applySort(list, sort) {
  switch (sort) {
    case 'name-asc':
      return sortBy(list, (p) => p.name, 'asc');
    case 'name-desc':
      return sortBy(list, (p) => p.name, 'desc');
    case 'budget-desc':
      return sortBy(list, (p) => p.budget ?? 0, 'desc');
    case 'budget-asc':
      return sortBy(list, (p) => p.budget ?? 0, 'asc');
    case 'deadline-asc':
      return sortBy(list, (p) => new Date(p.deadline), 'asc');
    case 'deadline-desc':
      return sortBy(list, (p) => new Date(p.deadline), 'desc');
    case 'created-desc':
      return sortBy(list, (p) => new Date(p.createdAt), 'desc');
    case 'created-asc':
      return sortBy(list, (p) => new Date(p.createdAt), 'asc');
    case 'status-asc':
      return sortBy(list, (p) => STATUS_ORDER.indexOf(resolveStatus(p.status).key), 'asc');
    default:
      return list;
  }
}

export default function ProjectListPage() {
  const { token, role, userId } = useAuth();
  const canCreate = role === 'Admin' || role === 'ProjectManager';
  // Admin/PM see every project by default, so a "mine only" filter is useful.
  // TeamMember/Client are already scoped to their projects server-side.
  const canFilterMine = role === 'Admin' || role === 'ProjectManager';
  const [projects, setProjects] = useState([]);
  const [phase, setPhase] = useState('loading');
  const [errorMessage, setErrorMessage] = useState('');
  const [reloadKey, setReloadKey] = useState(0);
  const [showCreate, setShowCreate] = useState(false);
  const [mineOnly, setMineOnly] = useState(false);
  const [statusFilter, setStatusFilter] = useState('');
  const [sort, setSort] = useState('');
  const [page, setPage] = useState(1);

  useEffect(() => {
    let ignore = false;

    const request =
      mineOnly && userId ? getProjectsByUserId(userId, token) : getProjects(token);

    request
      .then((data) => {
        if (ignore) return;
        setProjects(Array.isArray(data) ? data : []);
        setErrorMessage('');
        setPhase('ready');
      })
      .catch((error) => {
        if (ignore) return;
        setErrorMessage(
          error && error.status === 401
            ? 'Your session has expired. Please sign in again to see the projects.'
            : 'Something went wrong while loading the projects. Check your connection and try again.'
        );
        setPhase('error');
      });

    return () => {
      ignore = true;
    };
  }, [token, userId, mineOnly, reloadKey]);

  const reload = () => {
    setPhase('loading');
    setErrorMessage('');
    setReloadKey((key) => key + 1);
  };

  const visibleProjects = useMemo(() => {
    const filtered = statusFilter
      ? projects.filter((p) => resolveStatus(p.status).key === statusFilter)
      : projects;
    return applySort(filtered, sort);
  }, [projects, statusFilter, sort]);

  const totalPages = Math.max(1, Math.ceil(visibleProjects.length / PAGE_SIZE));
  const safePage = Math.min(page, totalPages);
  const pageProjects = visibleProjects.slice((safePage - 1) * PAGE_SIZE, safePage * PAGE_SIZE);

  return (
    <section className="projects-page">
      <header className="projects-header">
        <div className="projects-title-row">
          <h1 className="projects-title">Projects</h1>
          {phase === 'ready' && visibleProjects.length > 0 && (
            <span className="projects-count">{visibleProjects.length}</span>
          )}
          {canCreate && (
            <button
              type="button"
              className="projects-create-button"
              onClick={() => setShowCreate(true)}
            >
              Create Project
            </button>
          )}
        </div>
        <p className="projects-subtitle">
          Every project at a glance &mdash; budget, current status and deadline.
        </p>
      </header>

      {phase === 'ready' && (
        <div className="projects-toolbar">
          {userId && canFilterMine && (
            <label className="pl-toggle">
              <input
                type="checkbox"
                checked={mineOnly}
                onChange={() => {
                  setPhase('loading');
                  setMineOnly((value) => !value);
                  setPage(1);
                }}
              />
              My projects only
            </label>
          )}

          <label className="pl-control">
            Status
            <select
              value={statusFilter}
              onChange={(event) => {
                setStatusFilter(event.target.value);
                setPage(1);
              }}
            >
              <option value="">All</option>
              {STATUS_ORDER.map((key) => (
                <option key={key} value={key}>
                  {STATUS_META[key].label}
                </option>
              ))}
            </select>
          </label>

          <label className="pl-control">
            Sort
            <select
              value={sort}
              onChange={(event) => {
                setSort(event.target.value);
                setPage(1);
              }}
            >
              {SORT_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </label>
        </div>
      )}

      {phase === 'loading' && (
        <>
          <p className="pl-visually-hidden" role="status">
            Loading projects
          </p>
          <ProjectListSkeleton />
        </>
      )}

      {phase === 'error' && (
        <ProjectsState variant="error" title={'We couldn’t load the projects'} onRetry={reload}>
          {errorMessage}
        </ProjectsState>
      )}

      {phase === 'ready' && projects.length === 0 && (
        <ProjectsState
          variant="empty"
          title={mineOnly ? 'No projects for you yet' : 'No projects yet'}
        >
          {mineOnly
            ? "You're not a member of any projects yet."
            : "When projects are created they’ll appear here."}
        </ProjectsState>
      )}

      {phase === 'ready' && projects.length > 0 && visibleProjects.length === 0 && (
        <ProjectsState variant="empty" title="No matches">
          No projects match the current filters.
        </ProjectsState>
      )}

      {phase === 'ready' && visibleProjects.length > 0 && (
        <>
          <div className="projects-grid">
            {pageProjects.map((project) => (
              <ProjectCard key={project.projectId ?? project.name} project={project} />
            ))}
          </div>

          {totalPages > 1 && (
            <nav className="projects-pagination" aria-label="Projects pagination">
              <button
                type="button"
                className="pagination-button"
                onClick={() => setPage((current) => Math.max(1, current - 1))}
                disabled={safePage <= 1}
              >
                Previous
              </button>
              <span className="pagination-status">
                Page {safePage} of {totalPages}
              </span>
              <button
                type="button"
                className="pagination-button"
                onClick={() => setPage((current) => Math.min(totalPages, current + 1))}
                disabled={safePage >= totalPages}
              >
                Next
              </button>
            </nav>
          )}
        </>
      )}

      {showCreate && (
        <Modal title="Create Project" onClose={() => setShowCreate(false)}>
          <ProjectForm
            token={token}
            onCancel={() => setShowCreate(false)}
            onCreated={() => {
              setShowCreate(false);
              reload();
            }}
          />
        </Modal>
      )}
    </section>
  );
}
