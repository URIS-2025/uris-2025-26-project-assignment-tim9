import { useState } from 'react';

const MOCK_TASKS = [
  { id: '1', title: 'Napraviti JWT middleware', status: 'ToDo', priority: 'High', assignee: 'Sara' },
  { id: '2', title: 'Dodati role-based authorization', status: 'InProgress', priority: 'Critical', assignee: 'Sara' },
  { id: '3', title: 'Testirati DELETE endpoint', status: 'Done', priority: 'Medium', assignee: 'Marko' },
  { id: '4', title: 'Dodati validaciju za Deadline', status: 'InProgress', priority: 'Low', assignee: 'Sara' },
  { id: '5', title: 'Napisati integracione testove', status: 'ToDo', priority: 'Medium', assignee: 'Ana' },
];

const COLUMNS = [
  { key: 'ToDo', label: 'To Do' },
  { key: 'InProgress', label: 'In Progress' },
  { key: 'Done', label: 'Done' },
];

const PRIORITY_COLORS = {
  Critical: 'var(--color-status-critical)',
  High: 'var(--color-status-critical)',
  Medium: 'var(--color-status-in-progress)',
  Low: 'var(--border)',
};

export default function TaskBoard({ workPackageId }) {
  const [tasks] = useState(MOCK_TASKS);

  return (
    <div>
      <h2>Task Board</h2>
      <div style={{ display: 'flex', gap: '16px', maxWidth: '900px', margin: '0 auto' }}>
        {COLUMNS.map((column) => (
          <div
            key={column.key}
            style={{
              flex: 1,
              background: 'var(--code-bg)',
              borderRadius: '8px',
              padding: '12px',
            }}
          >
            <h3 style={{ fontSize: '16px', marginTop: 0 }}>{column.label}</h3>
            {tasks
              .filter((task) => task.status === column.key)
              .map((task) => (
                <div
                  key={task.id}
                  style={{
                    background: 'var(--bg)',
                    border: '1px solid var(--border)',
                    borderLeft: `4px solid ${PRIORITY_COLORS[task.priority]}`,
                    borderRadius: '6px',
                    padding: '10px 12px',
                    marginBottom: '8px',
                    textAlign: 'left',
                  }}
                >
                  <p style={{ margin: 0, fontWeight: 600 }}>{task.title}</p>
                  <p style={{ margin: '4px 0 0', fontSize: '13px', color: 'var(--text)' }}>
                    {task.assignee} • {task.priority}
                  </p>
                </div>
              ))}
          </div>
        ))}
      </div>
    </div>
  );
}
