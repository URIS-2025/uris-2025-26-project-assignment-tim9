import { useState } from 'react';

const MOCK_TASK = {
  id: '2',
  title: 'Dodati role-based authorization',
  description: 'Implementirati [Authorize(Roles = "ProjectManager,Admin")] na svim write endpoint-ima.',
  status: 'InProgress',
  priority: 'Critical',
  assignee: 'Sara',
};

const MOCK_DEPENDENCIES = [
  {
    id: 'd1',
    blockerTask: { id: '1', title: 'Napraviti JWT middleware', status: 'Done', assignee: 'Sara' },
  },
  {
    id: 'd2',
    blockerTask: { id: '4', title: 'Dodati validaciju za Deadline', status: 'InProgress', assignee: 'Sara' },
  },
];

const MOCK_COMMENTS = [
  { id: 'c1', author: 'Marko', text: 'Da li ovo pokriva i Admin rolu za DELETE?', createdAt: '2026-08-20T10:15:00' },
  { id: 'c2', author: 'Sara', text: 'Da, dodala sam oba u isti atribut.', createdAt: '2026-08-20T11:02:00' },
];

const STATUS_COLORS = {
  Done: 'var(--color-status-done)',
  InProgress: 'var(--color-status-in-progress)',
  ToDo: 'var(--border)',
};

function DependencyList({ dependencies }) {
  return (
    <div style={{ marginTop: '16px' }}>
      <h3 style={{ fontSize: '16px' }}>Blokira ga</h3>
      {dependencies.length === 0 && <p style={{ fontSize: '14px', color: 'var(--text)' }}>Nema zavisnosti.</p>}
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

function CommentSection({ comments }) {
  const [newComment, setNewComment] = useState('');

  return (
    <div style={{ marginTop: '16px' }}>
      <h3 style={{ fontSize: '16px' }}>Komentari</h3>
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
            {new Date(comment.createdAt).toLocaleString('sr-RS')}
          </p>
        </div>
      ))}
      <textarea
        value={newComment}
        onChange={(e) => setNewComment(e.target.value)}
        placeholder="Dodaj komentar..."
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
    </div>
  );
}

export default function TaskDetails({ taskId }) {
  const task = MOCK_TASK;

  return (
    <div style={{ maxWidth: '600px', margin: '0 auto', textAlign: 'left' }}>
      <h2 style={{ textAlign: 'center' }}>Detalji zadatka</h2>
      <div
        style={{
          border: '1px solid var(--border)',
          borderLeft: `4px solid ${STATUS_COLORS[task.status]}`,
          borderRadius: '8px',
          padding: '16px',
        }}
      >
        <h3 style={{ margin: 0 }}>{task.title}</h3>
        <p style={{ color: 'var(--text)' }}>{task.description}</p>
        <p style={{ fontSize: '13px' }}>
          Status: <strong>{task.status}</strong> • Prioritet: <strong>{task.priority}</strong> • Zadužen:{' '}
          <strong>{task.assignee}</strong>
        </p>
      </div>

      <DependencyList dependencies={MOCK_DEPENDENCIES} />
      <CommentSection comments={MOCK_COMMENTS} />
    </div>
  );
}
