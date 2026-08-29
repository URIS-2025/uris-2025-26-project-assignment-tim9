const MOCK_SUBTASKS = [
  {
    id: '1',
    title: 'Design the database',
    status: 'Done',
    subTasks: [
      { id: '1-1', title: 'Define entities', status: 'Done', subTasks: [] },
      {
        id: '1-2',
        title: 'Create migrations',
        status: 'InProgress',
        subTasks: [
          { id: '1-2-1', title: 'Initial migration', status: 'Done', subTasks: [] },
          { id: '1-2-2', title: 'Add Deadline migration', status: 'ToDo', subTasks: [] },
        ],
      },
    ],
  },
  {
    id: '2',
    title: 'Implement authentication',
    status: 'ToDo',
    subTasks: [],
  },
];

const STATUS_COLORS = {
  Done: 'var(--color-status-done)',
  InProgress: 'var(--color-status-in-progress)',
  ToDo: 'var(--border)',
};

function SubTaskItem({ task, depth = 0 }) {
  return (
    <div style={{ marginLeft: depth * 20, marginTop: '6px' }}>
      <div
        style={{
          display: 'flex',
          alignItems: 'center',
          gap: '8px',
          border: '1px solid var(--border)',
          borderLeft: `4px solid ${STATUS_COLORS[task.status]}`,
          borderRadius: '6px',
          padding: '8px 12px',
          background: 'var(--bg)',
          textAlign: 'left',
        }}
      >
        <span>{task.title}</span>
        <span style={{ fontSize: '12px', color: 'var(--text)' }}>({task.status})</span>
      </div>
      {task.subTasks && task.subTasks.length > 0 && (
        <div>
          {task.subTasks.map((subTask) => (
            <SubTaskItem key={subTask.id} task={subTask} depth={depth + 1} />
          ))}
        </div>
      )}
    </div>
  );
}

export default function SubTaskTree({ taskId }) {
  return (
    <div>
      <h2>Sub-Tasks</h2>
      <div style={{ maxWidth: '600px', margin: '0 auto' }}>
        {MOCK_SUBTASKS.map((task) => (
          <SubTaskItem key={task.id} task={task} depth={0} />
        ))}
      </div>
    </div>
  );
}
