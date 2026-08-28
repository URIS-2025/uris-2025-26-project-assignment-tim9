import { useCallback, useEffect, useMemo, useState } from 'react';
import { useAuth } from '../auth/useAuth';
import { ApiError } from '../api/httpClient';
import { getProjectsByUser, getProjectById } from '../api/projectApi';
import { getWorkPackagesByProject } from '../api/workPackageApi';
import { getTaskById, getTasksByWorkPackage } from '../api/taskApi';
import { getAllTimelogs, deleteTimelog } from '../api/timelogApi';
import TotalHoursPanel from '../components/TotalHoursPanel';
import TimelogList from '../components/TimelogList';
import TimelogFormModal from '../components/TimelogFormModal';
import './TimelogsPage.css';

export default function TimelogsPage() {
  const { token, userId, username, role, logout } = useAuth();

  const [projects, setProjects] = useState([]);
  const [rawTimelogs, setRawTimelogs] = useState([]);
  const [projectNames, setProjectNames] = useState({});
  const [taskTitles, setTaskTitles] = useState({});

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [refreshToken, setRefreshToken] = useState(0);

  const [modalState, setModalState] = useState(null); // null | { editing: timelog|null }
  const [deletingId, setDeletingId] = useState(null);
  const [actionError, setActionError] = useState(null);

  // Both filters are applied server-side via GET /api/timelog's query params.
  // filterTaskId only makes sense once a project is picked (tasks belong to
  // a work package, which belongs to a project), so it's cleared whenever
  // filterProjectId changes.
  const [filterProjectId, setFilterProjectId] = useState('');
  const [filterTaskId, setFilterTaskId] = useState('');
  const [filterTasks, setFilterTasks] = useState([]);
  const [filterTasksLoading, setFilterTasksLoading] = useState(false);

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

  // Load the user's projects and their own timelogs.
  useEffect(() => {
    let cancelled = false;
    (async () => {
      setLoading(true);
      setError(null);
      try {
        const [myProjects, allTimelogs] = await Promise.all([
          getProjectsByUser(userId, token),
          getAllTimelogs(token, { projectId: filterProjectId, taskId: filterTaskId }),
        ]);
        if (cancelled) return;

        setProjects(myProjects);
        setProjectNames((prev) => {
          const next = { ...prev };
          myProjects.forEach((p) => {
            next[p.projectId] = p.name;
          });
          return next;
        });

        const mine = allTimelogs
          .filter((t) => t.loggedByUserId === userId)
          .sort((a, b) => new Date(b.date) - new Date(a.date));
        setRawTimelogs(mine);
      } catch (err) {
        if (cancelled) return;
        if (!handleAuthError(err)) {
          setError(err.message || 'Failed to load your timelogs.');
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [userId, token, refreshToken, filterProjectId, filterTaskId, handleAuthError]);

  function handleFilterProjectChange(projectId) {
    setFilterProjectId(projectId);
    // A task filter is meaningless without a project - drop the stale
    // selection from whichever project was previously filtered.
    setFilterTaskId('');
  }

  // Task options for the filter bar - mirrors TimelogFormModal's own
  // project -> work packages -> tasks lookup, scoped to whichever project
  // is currently filtered.
  useEffect(() => {
    if (!filterProjectId) return undefined;
    let cancelled = false;
    (async () => {
      setFilterTasksLoading(true);
      try {
        const workPackages = await getWorkPackagesByProject(filterProjectId, token);
        const taskLists = await Promise.all(
          workPackages.map((wp) => getTasksByWorkPackage(wp.workPackageId, token))
        );
        if (!cancelled) setFilterTasks(taskLists.flat());
      } catch {
        if (!cancelled) setFilterTasks([]);
      } finally {
        if (!cancelled) setFilterTasksLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [filterProjectId, token]);

  // Resolve display names for any project/task not already cached
  // (e.g. a project the user has since left, or a task from a work
  // package we haven't loaded through the create-form flow).
  useEffect(() => {
    const missingProjectIds = [...new Set(rawTimelogs.map((t) => t.projectId))].filter(
      (id) => !(id in projectNames)
    );
    const missingTaskIds = [...new Set(rawTimelogs.map((t) => t.taskId))].filter(
      (id) => !(id in taskTitles)
    );
    if (missingProjectIds.length === 0 && missingTaskIds.length === 0) return;

    let cancelled = false;
    (async () => {
      const [projectResults, taskResults] = await Promise.all([
        Promise.all(
          missingProjectIds.map((id) =>
            getProjectById(id, token)
              .then((p) => [id, p?.name || 'Unknown project'])
              .catch(() => [id, 'Unknown project'])
          )
        ),
        Promise.all(
          missingTaskIds.map((id) =>
            getTaskById(id, token)
              .then((t) => [id, t?.title || 'Unknown task'])
              .catch(() => [id, 'Unknown task'])
          )
        ),
      ]);
      if (cancelled) return;
      if (projectResults.length) {
        setProjectNames((prev) => ({ ...prev, ...Object.fromEntries(projectResults) }));
      }
      if (taskResults.length) {
        setTaskTitles((prev) => ({ ...prev, ...Object.fromEntries(taskResults) }));
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [rawTimelogs, projectNames, taskTitles, token]);

  const enrichedTimelogs = useMemo(
    () =>
      rawTimelogs.map((t) => ({
        ...t,
        projectName: projectNames[t.projectId] || '…',
        taskTitle: taskTitles[t.taskId] || '…',
      })),
    [rawTimelogs, projectNames, taskTitles]
  );

  const filterProjectOptions = useMemo(
    () => [...projects].sort((a, b) => a.name.localeCompare(b.name)),
    [projects]
  );

  // Tasks belong to whichever project is currently filtered - once the
  // filter changes, stale tasks from the previous project shouldn't flash
  // before the new fetch resolves.
  const visibleFilterTasks = filterProjectId ? filterTasks : [];

  function refresh() {
    setRefreshToken((n) => n + 1);
  }

  function handleSaved() {
    setModalState(null);
    refresh();
  }

  async function handleDelete(id) {
    if (!window.confirm('Delete this timelog entry?')) return;
    setActionError(null);
    setDeletingId(id);
    try {
      await deleteTimelog(id, token, userId);
      refresh();
    } catch (err) {
      if (!handleAuthError(err)) {
        setActionError(err.message || 'Could not delete this timelog.');
      }
    } finally {
      setDeletingId(null);
    }
  }

  return (
    <div className="timelogs-page">
      <header className="page-header">
        <div>
          <h1>Timelogs</h1>
          <p className="page-subtitle">Track hours across every project you're on.</p>
        </div>
        <div className="user-badge">
          <div>
            <span className="user-name">{username}</span>
            <span className="user-role">{role}</span>
          </div>
          <button type="button" className="secondary-button" onClick={logout}>
            Log out
          </button>
        </div>
      </header>

      {loading ? (
        <p className="status-hint">Loading your timelogs…</p>
      ) : error ? (
        <div className="form-message error">{error}</div>
      ) : (
        <>
          <TotalHoursPanel projects={projects} timelogs={rawTimelogs} />

          <section className="timelog-section">
            <div className="timelog-section-header">
              <h2>Your timelogs</h2>
              <button
                type="button"
                className="fab"
                title="Log time"
                aria-label="Log time"
                onClick={() => setModalState({ editing: null })}
              >
                +
              </button>
            </div>

            <div className="timelog-filters">
              <label>
                Project
                <select
                  value={filterProjectId}
                  onChange={(e) => handleFilterProjectChange(e.target.value)}
                >
                  <option value="">All projects</option>
                  {filterProjectOptions.map((p) => (
                    <option key={p.projectId} value={p.projectId}>
                      {p.name}
                    </option>
                  ))}
                </select>
              </label>

              <label>
                Task
                <select
                  value={filterTaskId}
                  disabled={!filterProjectId || filterTasksLoading}
                  onChange={(e) => setFilterTaskId(e.target.value)}
                >
                  <option value="">
                    {filterTasksLoading
                      ? 'Loading tasks…'
                      : !filterProjectId
                        ? 'Pick a project first'
                        : 'All tasks'}
                  </option>
                  {visibleFilterTasks.map((t) => (
                    <option key={t.taskId} value={t.taskId}>
                      {t.title}
                    </option>
                  ))}
                </select>
              </label>
            </div>

            {actionError && <div className="form-message error">{actionError}</div>}

            <TimelogList
              timelogs={enrichedTimelogs}
              onEdit={(log) => setModalState({ editing: log })}
              onDelete={handleDelete}
              deletingId={deletingId}
            />
          </section>
        </>
      )}

      {modalState && (
        <TimelogFormModal
          projects={projects}
          editingTimelog={modalState.editing}
          onClose={() => setModalState(null)}
          onSaved={handleSaved}
        />
      )}
    </div>
  );
}
