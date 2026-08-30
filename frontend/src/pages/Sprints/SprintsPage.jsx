import { useCallback, useEffect, useMemo, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useAuth } from '../../auth/useAuth';
import { ApiError } from '../../api/httpClient';
import { getAllProjects } from '../../api/projectApi';
import { getAllSprints, deleteSprint } from '../../api/sprintApi';
import { deleteTask, getTasksBySprint } from '../../api/taskApi';
import { SPRINT_STATUSES, labelFor } from '../../shared/enums';
import TaskList from '../../components/TaskList';
import SprintFormModal from '../../components/SprintFormModal';
import TaskFormModal from '../../components/TaskFormModal';
import './SprintsPage.css';

function formatDate(iso) {
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return iso;
  return d.toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
}

// SprintController's create-sprint and TaskController's create-task routes
// are both [Authorize(Roles = "Admin,ProjectManager")] - keep this in sync
// with those if the backend's allowed roles ever change.
const CAN_MANAGE_ROLES = ['Admin', 'ProjectManager'];

export default function SprintsPage() {
  const { token, role, logout } = useAuth();
  const canManage = CAN_MANAGE_ROLES.includes(role);

  // Present when mounted at /projects/:projectId/sprints (a project-scoped
  // view) - absent at the flat /sprints route (every sprint the caller can
  // see, across every project). Same page either way, just scoped to fewer
  // sprints and a fixed project when opening the create form.
  const { projectId: routeProjectId } = useParams();

  const [projects, setProjects] = useState([]);
  const [sprints, setSprints] = useState([]);
  const [selectedSprintId, setSelectedSprintId] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [sprintsVersion, setSprintsVersion] = useState(0);
  const [deletingSprintId, setDeletingSprintId] = useState(null);

  // Tasks per sprint, keyed by sprint id - populated for whichever sprints
  // are currently on screen (the visible page of cards, plus the open
  // sprint's detail view). tasksVersion isn't read directly; it's how
  // creating/deleting a task forces a refetch.
  const [tasksBySprintId, setTasksBySprintId] = useState({});
  const [tasksLoading, setTasksLoading] = useState(false);
  const [tasksVersion, setTasksVersion] = useState(0);
  const [deletingTaskId, setDeletingTaskId] = useState(null);
  const [taskActionError, setTaskActionError] = useState(null);

  const [showSprintForm, setShowSprintForm] = useState(false);
  const [editingSprint, setEditingSprint] = useState(null);
  const [showTaskForm, setShowTaskForm] = useState(false);
  const [editingTask, setEditingTask] = useState(null);

  // Sprints are paged 3-at-a-time, stepping one sprint per arrow click
  // rather than jumping a full page at a time.
  const PAGE_SIZE = 3;
  const [pageStart, setPageStart] = useState(0);

  const handleAuthError = useCallback(
    (err) => {
      if (err instanceof ApiError && err.status === 401) {
        logout();
        return true;
      }
      return false;
    },
    [logout]
  );

  // Load every project (to attach sprints to) and every sprint that exists.
  useEffect(() => {
    let cancelled = false;
    (async () => {
      setLoading(true);
      setError(null);
      try {
        const [allProjects, allSprints] = await Promise.all([
          getAllProjects(token),
          getAllSprints(token, { projectId: routeProjectId }),
        ]);
        if (cancelled) return;
        setProjects(allProjects);
        // Closest end date first, so upcoming/wrapping-up sprints lead.
        const sorted = [...allSprints].sort(
          (a, b) => new Date(a.endDate) - new Date(b.endDate)
        );
        setSprints(sorted);
        // Drop the open sprint back to the list view if it no longer exists
        // (e.g. it was just deleted); otherwise leave the current view alone.
        setSelectedSprintId((prev) => (prev && sorted.some((s) => s.id === prev) ? prev : null));
      } catch (err) {
        if (!cancelled && !handleAuthError(err)) {
          setError(err.message || 'Failed to load sprints.');
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [token, sprintsVersion, routeProjectId, handleAuthError]);

  const projectNames = useMemo(
    () => Object.fromEntries(projects.map((p) => [p.projectId, p.name])),
    [projects]
  );

  const scopedProject = useMemo(
    () => (routeProjectId ? projects.find((p) => p.projectId === routeProjectId) : null),
    [routeProjectId, projects]
  );

  const selectedSprint = sprints.find((s) => s.id === selectedSprintId) ?? null;
  const tasks = selectedSprintId ? tasksBySprintId[selectedSprintId] || [] : [];

  // Clamp defensively so a stale pageStart (e.g. right after a delete
  // shrinks the list) never slices past the end.
  const maxPageStart = Math.max(0, sprints.length - PAGE_SIZE);
  const visibleStart = Math.min(pageStart, maxPageStart);
  const visibleSprints = sprints.slice(visibleStart, visibleStart + PAGE_SIZE);
  const canGoPrev = visibleStart > 0;
  const canGoNext = visibleStart < maxPageStart;

  // Real per-sprint task lists, fetched from WorkPackageService's
  // GET /api/task/sprint/{sprintId} - only for sprints actually on screen
  // (the visible page of cards, plus whichever sprint's detail view is
  // open), not every sprint that exists.
  const idsToLoad = useMemo(() => {
    const ids = new Set(visibleSprints.map((s) => s.id));
    if (selectedSprintId) ids.add(selectedSprintId);
    return [...ids];
  }, [visibleSprints, selectedSprintId]);
  const idsToLoadKey = idsToLoad.join(',');

  useEffect(() => {
    if (idsToLoad.length === 0) return undefined;
    let cancelled = false;
    (async () => {
      setTasksLoading(true);
      try {
        const entries = await Promise.all(
          idsToLoad.map(async (id) => [id, await getTasksBySprint(id, token)])
        );
        if (!cancelled) {
          setTasksBySprintId((prev) => ({ ...prev, ...Object.fromEntries(entries) }));
        }
      } catch (err) {
        if (!cancelled && !handleAuthError(err)) {
          setTaskActionError(err.message || 'Could not load tasks for one or more sprints.');
        }
      } finally {
        if (!cancelled) setTasksLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps -- idsToLoadKey is the stable form of idsToLoad
  }, [idsToLoadKey, tasksVersion, token, handleAuthError]);

  function goToPrevSprint() {
    setPageStart((p) => Math.max(0, Math.min(p, maxPageStart) - 1));
  }

  function goToNextSprint() {
    setPageStart((p) => Math.min(maxPageStart, Math.min(p, maxPageStart) + 1));
  }

  function handleSprintSaved(saved) {
    setShowSprintForm(false);
    setEditingSprint(null);
    setSprintsVersion((v) => v + 1);
    setSelectedSprintId(saved.id);
  }

  async function handleDeleteSprint(sprint) {
    if (!window.confirm(`Delete sprint "${sprint.name}"? This cannot be undone.`)) return;
    setError(null);
    setDeletingSprintId(sprint.id);
    try {
      await deleteSprint(sprint.id, token);
      if (selectedSprintId === sprint.id) setSelectedSprintId(null);
      setSprintsVersion((v) => v + 1);
    } catch (err) {
      if (!handleAuthError(err)) {
        setError(err.message || 'Could not delete this sprint.');
      }
    } finally {
      setDeletingSprintId(null);
    }
  }

  function handleTaskSaved() {
    setShowTaskForm(false);
    setEditingTask(null);
    setTasksVersion((v) => v + 1);
  }

  async function handleDeleteTask(task) {
    if (!window.confirm(`Delete task "${task.title}"?`)) return;
    setTaskActionError(null);
    setDeletingTaskId(task.taskId);
    try {
      await deleteTask(task.taskId, token);
      setTasksVersion((v) => v + 1);
    } catch (err) {
      if (!handleAuthError(err)) {
        setTaskActionError(err.message || 'Could not delete this task.');
      }
    } finally {
      setDeletingTaskId(null);
    }
  }

  return (
    <div className="sprints-page">
      {routeProjectId && (
        <Link to={`/projects/${routeProjectId}`} className="back-button">
          ← Back to project
        </Link>
      )}

      <header className="page-header">
        <div>
          <h1>Sprints</h1>
          <p className="page-subtitle">
            {routeProjectId
              ? `Sprints on ${scopedProject?.name || 'this project'}.`
              : 'Plan sprints per project and track their tasks.'}
          </p>
        </div>
      </header>

      {loading ? (
        <p className="status-hint">Loading sprints…</p>
      ) : error ? (
        <div className="form-message error">{error}</div>
      ) : selectedSprint ? (
        <section className="sprint-details">
          <button
            type="button"
            className="back-button"
            onClick={() => setSelectedSprintId(null)}
          >
            ← Back to sprints
          </button>

          <div className="sprint-details-header">
            <div>
              <h3>{selectedSprint.name}</h3>
              <p className="sprint-meta">
                {projectNames[selectedSprint.projectId] || 'Unknown project'} ·{' '}
                {labelFor(SPRINT_STATUSES, selectedSprint.status)} ·{' '}
                {formatDate(selectedSprint.startDate)} – {formatDate(selectedSprint.endDate)}
              </p>
            </div>
            {canManage && (
              <div className="sprint-details-actions">
                <button
                  type="button"
                  className="icon-button"
                  onClick={() => setEditingSprint(selectedSprint)}
                >
                  Edit sprint
                </button>
                <button
                  type="button"
                  className="icon-button danger"
                  disabled={deletingSprintId === selectedSprint.id}
                  onClick={() => handleDeleteSprint(selectedSprint)}
                >
                  {deletingSprintId === selectedSprint.id ? 'Deleting…' : 'Delete sprint'}
                </button>
              </div>
            )}
          </div>

          <div className="sprint-tasks-toolbar">
            <h4>Tasks in this sprint</h4>
            {canManage && (
              <button
                type="button"
                className="primary-button"
                onClick={() => setShowTaskForm(true)}
              >
                + New task
              </button>
            )}
          </div>

          {taskActionError && <div className="form-message error">{taskActionError}</div>}

          {tasksLoading && !tasksBySprintId[selectedSprint.id] ? (
            <p className="status-hint">Loading tasks…</p>
          ) : (
            <TaskList
              tasks={tasks}
              projectId={selectedSprint.projectId}
              onEdit={canManage ? setEditingTask : undefined}
              onDelete={canManage ? handleDeleteTask : undefined}
              deletingId={deletingTaskId}
            />
          )}
        </section>
      ) : (
        <>
          <div className="sprints-toolbar">
            <h2>Current sprints</h2>
            {canManage && (
              <button
                type="button"
                className="fab"
                title="New sprint"
                aria-label="New sprint"
                onClick={() => setShowSprintForm(true)}
              >
                +
              </button>
            )}
          </div>

          {sprints.length === 0 ? (
            <div className="sprint-list-empty">
              {canManage
                ? 'No sprints yet. Use the + button to create the first one.'
                : 'No sprints yet.'}
            </div>
          ) : (
            <div className="sprint-pager">
              <button
                type="button"
                className="pager-arrow"
                aria-label="Previous sprint"
                disabled={!canGoPrev}
                onClick={goToPrevSprint}
              >
                ‹
              </button>

              <div className="sprint-grid">
                {visibleSprints.map((sprint) => {
                  const sprintTasks = tasksBySprintId[sprint.id] || [];
                  return (
                    <div
                      key={sprint.id}
                      className="sprint-card"
                      role="button"
                      tabIndex={0}
                      onClick={() => setSelectedSprintId(sprint.id)}
                      onKeyDown={(e) => {
                        if (e.key === 'Enter' || e.key === ' ') {
                          e.preventDefault();
                          setSelectedSprintId(sprint.id);
                        }
                      }}
                    >
                      {canManage && (
                        <button
                          type="button"
                          className="sprint-card-delete"
                          aria-label={`Delete sprint ${sprint.name}`}
                          disabled={deletingSprintId === sprint.id}
                          onClick={(e) => {
                            e.stopPropagation();
                            handleDeleteSprint(sprint);
                          }}
                        >
                          ×
                        </button>
                      )}
                      <div className="sprint-card-body">
                        <span className="sprint-card-name">{sprint.name}</span>
                        <span className="badge outline">
                          {labelFor(SPRINT_STATUSES, sprint.status)}
                        </span>
                        <span className="sprint-card-project">
                          {projectNames[sprint.projectId] || 'Unknown project'}
                        </span>
                      </div>
                      <span className="sprint-card-preview">
                        {sprintTasks.length === 0
                          ? 'No tasks yet'
                          : `${sprintTasks.length} task${sprintTasks.length === 1 ? '' : 's'} · ${
                              sprintTasks[0].title
                            }${sprintTasks.length > 1 ? ', …' : ''}`}
                      </span>
                    </div>
                  );
                })}
              </div>

              <button
                type="button"
                className="pager-arrow"
                aria-label="Next sprint"
                disabled={!canGoNext}
                onClick={goToNextSprint}
              >
                ›
              </button>
            </div>
          )}
        </>
      )}

      {(showSprintForm || editingSprint) && (
        <SprintFormModal
          projects={projects}
          sprint={editingSprint}
          initialProjectId={routeProjectId}
          onClose={() => {
            setShowSprintForm(false);
            setEditingSprint(null);
          }}
          onSaved={handleSprintSaved}
        />
      )}

      {(showTaskForm || editingTask) && selectedSprint && (
        <TaskFormModal
          sprintId={selectedSprint.id}
          projectId={selectedSprint.projectId}
          task={editingTask}
          onClose={() => {
            setShowTaskForm(false);
            setEditingTask(null);
          }}
          onSaved={handleTaskSaved}
        />
      )}
    </div>
  );
}
