import { useState } from 'react';
import './TaskBoard.css';

const MOCK_TASKS = [
  { id: '1', title: 'Build JWT middleware', status: 'ToDo', priority: 'High', assignee: 'Sara' },
  { id: '2', title: 'Add role-based authorization', status: 'InProgress', priority: 'Critical', assignee: 'Sara' },
  { id: '3', title: 'Test the DELETE endpoint', status: 'Done', priority: 'Medium', assignee: 'Marko' },
  { id: '4', title: 'Add validation for Deadline', status: 'InProgress', priority: 'Low', assignee: 'Sara' },
  { id: '5', title: 'Write integration tests', status: 'ToDo', priority: 'Medium', assignee: 'Ana' },
];

const COLUMNS = [
  { key: 'ToDo', label: 'To Do' },
  { key: 'InProgress', label: 'In Progress' },
  { key: 'Done', label: 'Done' },
];

const STATUS_OPTIONS = [
  { value: 'ToDo', label: 'To Do' },
  { value: 'InProgress', label: 'In Progress' },
  { value: 'Done', label: 'Done' },
];

const PRIORITY_COLORS = {
  Critical: 'var(--color-status-critical)',
  High: 'var(--color-status-critical)',
  Medium: 'var(--color-status-in-progress)',
  Low: 'var(--border)',
};

export default function TaskBoard({ workPackageId, onTaskClick }) {
  const [tasks, setTasks] = useState(MOCK_TASKS);

  function handleAdd(columnKey) {
    const title = window.prompt('New task name:');
    if (!title || !title.trim()) return;

    setTasks((prev) => [
      ...prev,
      {
        id: crypto.randomUUID(),
        title: title.trim(),
        status: columnKey,
        priority: 'Medium',
        assignee: '—',
      },
    ]);
  }

  function handleStatusChange(id, status) {
    setTasks((prev) => prev.map((task) => (task.id === id ? { ...task, status } : task)));
  }

  function handleDelete(id) {
    if (!window.confirm('Delete this task?')) return;
    setTasks((prev) => prev.filter((task) => task.id !== id));
  }

  return (
    <div className="task-board" data-work-package-id={workPackageId}>
      <h2>Task Board</h2>

      <div className="task-board__columns">
        {COLUMNS.map((column) => (
          <div key={column.key} className="task-column">
            <div className="task-column__header">
              <h3>{column.label}</h3>
              <button
                type="button"
                className="task-column__add"
                aria-label={`Add task to ${column.label} column`}
                onClick={() => handleAdd(column.key)}
              >
                +
              </button>
            </div>

            {tasks
              .filter((task) => task.status === column.key)
              .map((task) => (
                <div
                  key={task.id}
                  className="task-card"
                  style={{ borderLeftColor: PRIORITY_COLORS[task.priority] ?? 'var(--border)' }}
                  onClick={() => onTaskClick?.(task)}
                >
                  <button
                    type="button"
                    className="task-card__delete"
                    aria-label="Delete task"
                    onClick={(event) => {
                      event.stopPropagation();
                      handleDelete(task.id);
                    }}
                  >
                    ×
                  </button>

                  <p className="task-card__title">{task.title}</p>
                  <p className="task-card__meta">
                    {task.assignee} • {task.priority}
                  </p>

                  <select
                    className="task-card__status"
                    value={task.status}
                    onClick={(event) => event.stopPropagation()}
                    onChange={(event) => handleStatusChange(task.id, event.target.value)}
                  >
                    {STATUS_OPTIONS.map((option) => (
                      <option key={option.value} value={option.value}>
                        {option.label}
                      </option>
                    ))}
                  </select>
                </div>
              ))}
          </div>
        ))}
      </div>
    </div>
  );
}
