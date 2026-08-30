import { useEffect, useMemo, useState } from 'react';
import { useAuth } from '../auth/useAuth';
import {
  getTask,
  getTasks,
  getComments,
  getDependencies,
  addComment,
  updateComment,
  deleteComment,
  addDependency,
  deleteDependency,
  reassignTask,
  moveTask,
  getWorkPackage,
  getWorkPackages,
  TASK_STATUS_LABELS,
  TASK_PRIORITY_LABELS,
} from '../api/workPackageApi';
import { getProjectMembersByProjectId } from '../api/projectApi';
import { useUserNames, shortId } from '../utils/userNames';
import { getFriendlyErrorMessage } from '../utils/errorMessages';
import { useToast } from '../shared/components/useToast';
import Modal from './Modal';
import SubTaskTree from './SubTaskTree';

const STATUS_COLORS = {
  Done: 'var(--color-status-done)',
  InProgress: 'var(--color-status-in-progress)',
  InReview: 'var(--color-status-in-progress)',
  ToDo: 'var(--border)',
  Blocked: 'var(--color-status-critical)',
};

const fieldStyle = {
  padding: '6px 8px',
  borderRadius: '6px',
  border: '1px solid var(--border)',
  fontFamily: 'var(--sans)',
  fontSize: '13px',
  boxSizing: 'border-box',
};

const smallButton = {
  padding: '4px 10px',
  fontSize: '12px',
  fontFamily: 'var(--sans)',
  borderRadius: '6px',
  border: '1px solid var(--border)',
  background: 'transparent',
  color: 'var(--text)',
  cursor: 'pointer',
};

const smallPrimary = {
  ...smallButton,
  border: '1px solid var(--accent)',
  background: 'var(--accent)',
  color: '#fff',
};

function DependencySection({ taskId, workPackageId, token, canManage }) {
  const { showToast } = useToast();
  const [dependencies, setDependencies] = useState([]);
  const [candidates, setCandidates] = useState([]);
  const [selected, setSelected] = useState('');
  const [busy, setBusy] = useState(false); // a remove is in flight
  const [adding, setAdding] = useState(false); // the add call is in flight
  const [error, setError] = useState(''); // load-time error only
  const [version, setVersion] = useState(0);

  useEffect(() => {
    let ignore = false;
    Promise.all([getDependencies(taskId, token), getTasks(workPackageId, token)])
      .then(async ([deps, wpTasks]) => {
        const blockers = await Promise.all(
          (deps || []).map((d) => getTask(d.blockerTaskId, token).catch(() => null)),
        );
        if (ignore) return;
        setError('');
        setDependencies(
          (deps || []).map((dep, i) => ({
            id: dep.dependencyId,
            blockerTaskId: dep.blockerTaskId,
            title: blockers[i]?.title ?? `Task ${String(dep.blockerTaskId).slice(0, 8)}…`,
            status: blockers[i] ? TASK_STATUS_LABELS[blockers[i].status] ?? String(blockers[i].status) : '—',
          })),
        );
        setCandidates((wpTasks || []).filter((t) => t.taskId !== taskId));
      })
      .catch(() => {
        if (!ignore) setError('Could not load dependencies.');
      });
    return () => {
      ignore = true;
    };
  }, [taskId, workPackageId, token, version]);

  async function handleAdd() {
    if (!selected || adding) return;
    setAdding(true);
    try {
      const created = await addDependency(taskId, selected, token);
      // Show it immediately from the create response - don't wait on the
      // refetch below, which also runs a parallel getTasks() that could fail
      // and (via the effect's catch) leave a successful add invisible.
      const blocker = candidates.find((t) => t.taskId === selected);
      const newId = created?.dependencyId ?? `pending-${selected}`;
      setDependencies((prev) =>
        prev.some((d) => d.blockerTaskId === selected)
          ? prev
          : [
              ...prev,
              {
                id: newId,
                blockerTaskId: selected,
                title: blocker?.title ?? `Task ${String(selected).slice(0, 8)}…`,
                status: blocker
                  ? TASK_STATUS_LABELS[blocker.status] ?? String(blocker.status)
                  : '—',
              },
            ],
      );
      setSelected('');
      setVersion((v) => v + 1); // reconcile with the server
      showToast('Dependency added.', 'success');
    } catch (err) {
      showToast(getFriendlyErrorMessage(err, 'dependency-write'), 'error');
    } finally {
      setAdding(false);
    }
  }

  async function handleDelete(dependencyId) {
    setBusy(true);
    try {
      await deleteDependency(dependencyId, token);
      setDependencies((prev) => prev.filter((d) => d.id !== dependencyId));
      setVersion((v) => v + 1);
      showToast('Dependency removed.', 'success');
    } catch (err) {
      showToast(getFriendlyErrorMessage(err, 'dependency-write'), 'error');
    } finally {
      setBusy(false);
    }
  }

  const alreadyLinked = new Set(dependencies.map((d) => d.blockerTaskId));

  return (
    <div style={{ marginTop: '16px' }}>
      <h3 style={{ fontSize: '16px' }}>Blocked by</h3>
      {dependencies.length === 0 && (
        <p style={{ fontSize: '14px', color: 'var(--text)' }}>No dependencies.</p>
      )}
      {dependencies.map((dep) => {
        const isBlocking = dep.status !== 'Done';
        return (
          <div
            key={dep.id}
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: '8px',
              border: '1px solid var(--border)',
              borderLeft: `4px solid ${isBlocking ? 'var(--color-status-critical)' : 'var(--color-status-done)'}`,
              borderRadius: '6px',
              padding: '8px 12px',
              marginBottom: '6px',
              textAlign: 'left',
              background: isBlocking ? 'var(--accent-bg)' : 'var(--bg)',
            }}
          >
            <span>{dep.title}</span>
            <span style={{ fontSize: '12px', color: STATUS_COLORS[dep.status] }}>({dep.status})</span>
            {canManage && (
              <button
                type="button"
                aria-label="Remove dependency"
                onClick={() => handleDelete(dep.id)}
                disabled={busy || adding}
                style={{ ...smallButton, marginLeft: 'auto', color: 'var(--color-status-critical)' }}
              >
                ×
              </button>
            )}
          </div>
        );
      })}

      {canManage && (
        <div style={{ display: 'flex', gap: '6px', flexWrap: 'wrap', marginTop: '8px' }}>
          <select
            value={selected}
            onChange={(e) => setSelected(e.target.value)}
            style={{ ...fieldStyle, flex: '1 1 200px' }}
          >
            <option value="">Select a blocking task…</option>
            {candidates
              .filter((t) => !alreadyLinked.has(t.taskId))
              .map((t) => (
                <option key={t.taskId} value={t.taskId}>
                  {t.title}
                </option>
              ))}
          </select>
          <button
            type="button"
            onClick={handleAdd}
            disabled={adding || busy || !selected}
            style={smallPrimary}
          >
            {adding ? 'Adding…' : 'Add dependency'}
          </button>
        </div>
      )}
      {error && <p style={{ fontSize: '12px', color: 'var(--color-status-critical)' }}>{error}</p>}
    </div>
  );
}

function CommentSection({ taskId, token, userId }) {
  const [comments, setComments] = useState([]);
  const [newComment, setNewComment] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [editText, setEditText] = useState('');
  const [error, setError] = useState('');
  const [version, setVersion] = useState(0);

  useEffect(() => {
    let ignore = false;
    getComments(taskId, token)
      .then((data) => {
        if (!ignore) setComments(Array.isArray(data) ? data : []);
      })
      .catch(() => {
        if (!ignore) setError('Could not load comments.');
      });
    return () => {
      ignore = true;
    };
  }, [taskId, token, version]);

  const authorIds = useMemo(() => comments.map((c) => c.authorId), [comments]);
  const nameFor = useUserNames(authorIds, token);

  async function handleSubmit(event) {
    event.preventDefault();
    const text = newComment.trim();
    if (!text) return;
    setSubmitting(true);
    setError('');
    try {
      await addComment(taskId, text, token, userId);
      setNewComment('');
      setVersion((v) => v + 1);
    } catch (err) {
      setError(getFriendlyErrorMessage(err, 'comment-create'));
    } finally {
      setSubmitting(false);
    }
  }

  async function handleEditSave(commentId) {
    const text = editText.trim();
    if (!text) return;
    setError('');
    try {
      await updateComment(commentId, text, token, userId);
      setEditingId(null);
      setEditText('');
      setVersion((v) => v + 1);
    } catch (err) {
      setError(getFriendlyErrorMessage(err, 'comment-edit'));
    }
  }

  async function handleDelete(commentId) {
    if (!window.confirm('Delete this comment?')) return;
    setError('');
    try {
      await deleteComment(commentId, token, userId);
      setVersion((v) => v + 1);
    } catch (err) {
      setError(getFriendlyErrorMessage(err, 'comment-edit'));
    }
  }

  return (
    <div style={{ marginTop: '16px' }}>
      <h3 style={{ fontSize: '16px' }}>Comments</h3>
      {comments.map((comment) => {
        const isAuthor = comment.authorId === userId;
        const isEditing = editingId === comment.commentId;
        return (
          <div
            key={comment.commentId}
            style={{
              border: '1px solid var(--border)',
              borderRadius: '6px',
              padding: '8px 12px',
              marginBottom: '6px',
              textAlign: 'left',
              background: 'var(--bg)',
            }}
          >
            <p style={{ margin: 0, fontWeight: 600, fontSize: '14px' }}>
              {isAuthor ? 'You' : nameFor(comment.authorId)}
            </p>
            {isEditing ? (
              <>
                <textarea
                  value={editText}
                  onChange={(e) => setEditText(e.target.value)}
                  style={{ ...fieldStyle, width: '100%', minHeight: '50px', marginTop: '4px' }}
                />
                <div style={{ display: 'flex', gap: '6px', marginTop: '4px' }}>
                  <button type="button" style={smallPrimary} onClick={() => handleEditSave(comment.commentId)}>
                    Save
                  </button>
                  <button
                    type="button"
                    style={smallButton}
                    onClick={() => {
                      setEditingId(null);
                      setEditText('');
                    }}
                  >
                    Cancel
                  </button>
                </div>
              </>
            ) : (
              <>
                <p style={{ margin: '2px 0', fontSize: '14px' }}>{comment.text}</p>
                <p style={{ margin: 0, fontSize: '12px', color: 'var(--text)' }}>
                  {new Date(comment.createdAt).toLocaleString('en-GB')}
                  {comment.updatedAt ? ' (edited)' : ''}
                </p>
                {isAuthor && (
                  <div style={{ display: 'flex', gap: '6px', marginTop: '4px' }}>
                    <button
                      type="button"
                      style={smallButton}
                      onClick={() => {
                        setEditingId(comment.commentId);
                        setEditText(comment.text);
                      }}
                    >
                      ✎ Edit
                    </button>
                    <button
                      type="button"
                      style={{ ...smallButton, color: 'var(--color-status-critical)' }}
                      onClick={() => handleDelete(comment.commentId)}
                    >
                      Delete
                    </button>
                  </div>
                )}
              </>
            )}
          </div>
        );
      })}

      <form onSubmit={handleSubmit}>
        <textarea
          value={newComment}
          onChange={(e) => setNewComment(e.target.value)}
          placeholder="Add a comment..."
          style={{
            width: '100%',
            minHeight: '60px',
            marginTop: '8px',
            padding: '8px',
            borderRadius: '6px',
            border: '1px solid var(--border)',
            fontFamily: 'var(--sans)',
            boxSizing: 'border-box',
          }}
        />
        <button
          type="submit"
          disabled={submitting || !newComment.trim()}
          style={{
            marginTop: '8px',
            background: 'var(--accent)',
            color: '#fff',
            border: 'none',
            borderRadius: '6px',
            padding: '8px 16px',
            cursor: 'pointer',
            fontFamily: 'var(--sans)',
          }}
        >
          {submitting ? 'Posting…' : 'Post comment'}
        </button>
      </form>
      {error && <p style={{ fontSize: '12px', color: 'var(--color-status-critical)' }}>{error}</p>}
    </div>
  );
}

function ReassignMoveSection({ task, token, onChanged }) {
  const { showToast } = useToast();
  const [assigneeId, setAssigneeId] = useState('');
  const [workPackages, setWorkPackages] = useState([]);
  const [members, setMembers] = useState([]);
  const [moveTarget, setMoveTarget] = useState('');
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    let ignore = false;
    getWorkPackage(task.workPackageId, token)
      .then((wp) => {
        const projectId = wp?.projectId;
        if (!projectId) return [[], []];
        return Promise.all([
          getWorkPackages(projectId, token).catch(() => []),
          getProjectMembersByProjectId(projectId, token).catch(() => []),
        ]);
      })
      .then(([wps, mems]) => {
        if (ignore) return;
        setWorkPackages(Array.isArray(wps) ? wps : []);
        setMembers(Array.isArray(mems) ? mems : []);
      })
      .catch(() => {
        if (!ignore) {
          setWorkPackages([]);
          setMembers([]);
        }
      });
    return () => {
      ignore = true;
    };
  }, [task.workPackageId, token]);

  async function handleReassign() {
    if (!assigneeId) return;
    setBusy(true);
    try {
      await reassignTask(task.taskId, assigneeId, token);
      setAssigneeId('');
      showToast('Task reassigned.', 'success');
      onChanged?.();
    } catch (err) {
      showToast(getFriendlyErrorMessage(err, 'task-write'), 'error');
    } finally {
      setBusy(false);
    }
  }

  async function handleMove() {
    if (!moveTarget) return;
    setBusy(true);
    try {
      await moveTask(task.taskId, moveTarget, token);
      showToast('Task moved.', 'success');
      onChanged?.();
    } catch (err) {
      showToast(getFriendlyErrorMessage(err, 'task-write'), 'error');
    } finally {
      setBusy(false);
    }
  }

  return (
    <div style={{ marginTop: '16px' }}>
      <h3 style={{ fontSize: '16px' }}>Assignment</h3>
      <div style={{ display: 'flex', gap: '6px', flexWrap: 'wrap', marginBottom: '8px' }}>
        <select
          value={assigneeId}
          onChange={(e) => setAssigneeId(e.target.value)}
          style={{ ...fieldStyle, flex: '1 1 240px' }}
        >
          <option value="">
            {members.length ? 'Reassign to…' : 'No project members found'}
          </option>
          {members
            .filter((m) => m.userId !== task.assigneeId)
            .map((m) => (
              <option key={m.userId} value={m.userId}>
                {m.username || shortId(m.userId)}
              </option>
            ))}
        </select>
        <button type="button" onClick={handleReassign} disabled={busy || !assigneeId} style={smallPrimary}>
          Reassign
        </button>
      </div>
      <div style={{ display: 'flex', gap: '6px', flexWrap: 'wrap' }}>
        <select
          value={moveTarget}
          onChange={(e) => setMoveTarget(e.target.value)}
          style={{ ...fieldStyle, flex: '1 1 240px' }}
        >
          <option value="">Move to another work package…</option>
          {workPackages
            .filter((wp) => wp.workPackageId !== task.workPackageId)
            .map((wp) => (
              <option key={wp.workPackageId} value={wp.workPackageId}>
                {wp.name}
              </option>
            ))}
        </select>
        <button type="button" onClick={handleMove} disabled={busy || !moveTarget} style={smallPrimary}>
          Move
        </button>
      </div>
    </div>
  );
}

export default function TaskDetails({ taskId, onChanged }) {
  const { token, userId, role } = useAuth();
  const canManage = role === 'Admin' || role === 'ProjectManager';
  const [task, setTask] = useState(null);
  const [phase, setPhase] = useState('loading');
  const [errorMessage, setErrorMessage] = useState('');
  const [reloadKey, setReloadKey] = useState(0);
  const [showSubTasks, setShowSubTasks] = useState(false);
  const [subTask, setSubTask] = useState(null); // sub-task opened in a stacked modal

  useEffect(() => {
    let ignore = false;
    getTask(taskId, token)
      .then((data) => {
        if (ignore) return;
        setTask(data);
        setErrorMessage('');
        setPhase('ready');
      })
      .catch((error) => {
        if (ignore) return;
        setErrorMessage(
          error && error.status === 401
            ? 'Your session has expired. Please sign in again.'
            : 'Something went wrong while loading the task.',
        );
        setPhase('error');
      });
    return () => {
      ignore = true;
    };
  }, [taskId, token, reloadKey]);

  const nameIds = useMemo(
    () => (task ? [task.assigneeId, task.approverId] : []),
    [task],
  );
  const nameFor = useUserNames(nameIds, token);

  function handleChanged() {
    setReloadKey((k) => k + 1);
    onChanged?.();
  }

  if (phase === 'loading') return <p style={{ textAlign: 'center' }}>Loading...</p>;
  if (phase === 'error') return <p style={{ textAlign: 'center' }}>{errorMessage}</p>;
  if (!task) return null;

  const statusLabel = TASK_STATUS_LABELS[task.status] ?? String(task.status);
  const priorityLabel = TASK_PRIORITY_LABELS[task.priority] ?? String(task.priority);

  return (
    <>
    <div style={{ maxWidth: '600px', margin: '0 auto', textAlign: 'left' }}>
      <h2 style={{ textAlign: 'center' }}>Task Details</h2>
      <div
        style={{
          border: '1px solid var(--border)',
          borderLeft: `4px solid ${STATUS_COLORS[statusLabel] ?? 'var(--border)'}`,
          borderRadius: '8px',
          padding: '16px',
        }}
      >
        <h3 style={{ margin: 0 }}>{task.title}</h3>
        <p style={{ color: 'var(--text)' }}>{task.description}</p>
        <p style={{ fontSize: '13px' }}>
          Status: <strong>{statusLabel}</strong> • Priority: <strong>{priorityLabel}</strong> • Assignee:{' '}
          <strong>{task.assigneeId ? nameFor(task.assigneeId) : 'Unassigned'}</strong>
        </p>
      </div>

      <DependencySection
        taskId={taskId}
        workPackageId={task.workPackageId}
        token={token}
        canManage={canManage}
      />

      <CommentSection taskId={taskId} token={token} userId={userId} />

      {canManage && <ReassignMoveSection task={task} token={token} onChanged={handleChanged} />}

      <div style={{ marginTop: '16px' }}>
        <button
          type="button"
          style={{ ...smallButton }}
          onClick={() => setShowSubTasks((v) => !v)}
        >
          {showSubTasks ? '▾ Hide sub-tasks' : '▸ Show sub-tasks'}
        </button>
        {showSubTasks && (
          <div style={{ marginTop: '10px' }}>
            <SubTaskTree
              taskId={taskId}
              workPackageId={task.workPackageId}
              onTaskClick={setSubTask}
            />
          </div>
        )}
      </div>
    </div>

    {subTask && (
      <Modal title={subTask.title} onClose={() => setSubTask(null)}>
        <TaskDetails taskId={subTask.taskId} onChanged={handleChanged} />
      </Modal>
    )}
    </>
  );
}
