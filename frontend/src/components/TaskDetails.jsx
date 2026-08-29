import { useEffect, useState } from 'react';
import { useAuth } from '../auth/useAuth';
import {
  getTask,
  getComments,
  getDependencies,
  addComment,
  TASK_STATUS_LABELS,
  TASK_PRIORITY_LABELS,
} from '../api/workPackageApi';

const STATUS_COLORS = {
  Done: 'var(--color-status-done)',
  InProgress: 'var(--color-status-in-progress)',
  InReview: 'var(--color-status-in-progress)',
  ToDo: 'var(--border)',
  Blocked: 'var(--color-status-critical)',
};

function DependencyList({ dependencies }) {
  return (
    <div style={{ marginTop: '16px' }}>
      <h3 style={{ fontSize: '16px' }}>Blocked by</h3>
      {dependencies.length === 0 && <p style={{ fontSize: '14px', color: 'var(--text)' }}>No dependencies.</p>}
      {dependencies.map((dep) => {
        const isBlocking = dep.blockerTask.status !== 'Done';
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
            <span>{dep.blockerTask.title}</span>
            <span style={{ fontSize: '12px', color: STATUS_COLORS[dep.blockerTask.status] }}>
              ({dep.blockerTask.status})
            </span>
            <span style={{ fontSize: '12px', color: 'var(--text)' }}>— {dep.blockerTask.assignee}</span>
          </div>
        );
      })}
    </div>
  );
}

function CommentSection({ comments, onSubmit }) {
  const [newComment, setNewComment] = useState('');
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(event) {
    event.preventDefault();
    const text = newComment.trim();
    if (!text) return;
    setSubmitting(true);
    try {
      await onSubmit(text);
      setNewComment('');
    } catch (error) {
      window.alert(error?.message || 'Could not add the comment.');
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div style={{ marginTop: '16px' }}>
      <h3 style={{ fontSize: '16px' }}>Comments</h3>
      {comments.map((comment) => (
        <div
          key={comment.id}
          style={{
            border: '1px solid var(--border)',
            borderRadius: '6px',
            padding: '8px 12px',
            marginBottom: '6px',
            textAlign: 'left',
            background: 'var(--bg)',
          }}
        >
          <p style={{ margin: 0, fontWeight: 600, fontSize: '14px' }}>{comment.author}</p>
          <p style={{ margin: '2px 0', fontSize: '14px' }}>{comment.text}</p>
          <p style={{ margin: 0, fontSize: '12px', color: 'var(--text)' }}>
            {new Date(comment.createdAt).toLocaleString('en-GB')}
          </p>
        </div>
      ))}
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
    </div>
  );
}

export default function TaskDetails({ taskId }) {
  const { token, userId } = useAuth();
  const [task, setTask] = useState(null);
  const [dependencies, setDependencies] = useState([]);
  const [comments, setComments] = useState([]);
  const [phase, setPhase] = useState('loading');
  const [errorMessage, setErrorMessage] = useState('');

  useEffect(() => {
    let ignore = false;

    Promise.all([getTask(taskId, token), getDependencies(taskId, token), getComments(taskId, token)])
      .then(async ([taskData, depData, commentData]) => {
        // Dependency DTO only carries BlockerTaskId - fetch each blocker for its title/status.
        const blockers = await Promise.all(
          (depData || []).map((dep) =>
            getTask(dep.blockerTaskId, token).catch(() => null),
          ),
        );
        if (ignore) return;
        setTask(taskData);
        setDependencies(
          (depData || []).map((dep, index) => {
            const blocker = blockers[index];
            return {
              id: dep.dependencyId,
              blockerTask: {
                title: blocker?.title ?? `Task ${String(dep.blockerTaskId).slice(0, 8)}…`,
                status: blocker ? TASK_STATUS_LABELS[blocker.status] ?? String(blocker.status) : '—',
                assignee: blocker?.assigneeId ? `${String(blocker.assigneeId).slice(0, 8)}…` : 'Unassigned',
              },
            };
          }),
        );
        setComments(
          (commentData || []).map((comment) => ({
            id: comment.commentId,
            author: comment.authorId === userId ? 'You' : `${String(comment.authorId).slice(0, 8)}…`,
            text: comment.text,
            createdAt: comment.createdAt,
          })),
        );
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
  }, [taskId, token, userId]);

  async function handleAddComment(text) {
    const created = await addComment(taskId, text, token, userId);
    const fresh = await getComments(taskId, token).catch(() => null);
    if (Array.isArray(fresh) && fresh.length) {
      setComments(
        fresh.map((comment) => ({
          id: comment.commentId,
          author: comment.authorId === userId ? 'You' : `${String(comment.authorId).slice(0, 8)}…`,
          text: comment.text,
          createdAt: comment.createdAt,
        })),
      );
    } else {
      setComments((prev) => [
        ...prev,
        { id: created.commentId, author: 'You', text: created.text, createdAt: created.createdAt },
      ]);
    }
  }

  if (phase === 'loading') return <p style={{ textAlign: 'center' }}>Loading...</p>;
  if (phase === 'error') return <p style={{ textAlign: 'center' }}>{errorMessage}</p>;
  if (!task) return null;

  const statusLabel = TASK_STATUS_LABELS[task.status] ?? String(task.status);
  const priorityLabel = TASK_PRIORITY_LABELS[task.priority] ?? String(task.priority);

  return (
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
          <strong>{task.assigneeId ? `${String(task.assigneeId).slice(0, 8)}…` : 'Unassigned'}</strong>
        </p>
      </div>

      <DependencyList dependencies={dependencies} />
      <CommentSection comments={comments} onSubmit={handleAddComment} />
    </div>
  );
}
