import { TASK_STATUSES, TASK_PRIORITIES, labelFor } from '../shared/enums';
import AttachmentsButton from './AttachmentsButton';
import './TaskList.css';

function formatDate(iso) {
  if (!iso) return '—';
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return iso;
  return d.toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
}

const STATUS_CLASS = {
  ToDo: 'neutral',
  InProgress: 'in-progress',
  InReview: 'in-progress',
  Done: 'done',
  Blocked: 'critical',
};

/**
 * @param {object} props
 * @param {Array} props.tasks
 * @param {string} props.projectId - the sprint's project; every task here belongs to it
 *   (Task itself only carries a WorkPackageId, not a ProjectId - AttachmentService needs
 *   the project explicitly)
 * @param {(task: object) => void} [props.onEdit] - omit to hide the Edit button (e.g. for
 *   TeamMember/Client, who aren't allowed to edit tasks)
 * @param {(task: object) => void} [props.onDelete]
 * @param {string|null} [props.deletingId]
 */
export default function TaskList({ tasks, projectId, onEdit, onDelete, deletingId }) {
  if (tasks.length === 0) {
    return <div className="task-list-empty">No tasks in this sprint yet.</div>;
  }

  return (
    <div className="task-list">
      {tasks.map((task) => {
        const statusLabel = labelFor(TASK_STATUSES, task.status);
        return (
          <div className="task-row" key={task.taskId}>
            <div className="task-row-main">
              <span className="task-title">{task.title}</span>
              {task.description && <span className="task-description">{task.description}</span>}
            </div>
            <span className={`badge ${STATUS_CLASS[statusLabel] || 'neutral'}`}>{statusLabel}</span>
            <span className="badge outline">{labelFor(TASK_PRIORITIES, task.priority)}</span>
            <span className="task-due">{formatDate(task.dueDate)}</span>
            <span className="task-actions">
              <AttachmentsButton projectId={projectId} taskId={task.taskId} label="Files" />
              {onEdit && (
                <button type="button" className="icon-button" onClick={() => onEdit(task)}>
                  Edit
                </button>
              )}
              {onDelete && (
                <button
                  type="button"
                  className="icon-button danger"
                  disabled={deletingId === task.taskId}
                  onClick={() => onDelete(task)}
                >
                  {deletingId === task.taskId ? 'Deleting…' : 'Delete'}
                </button>
              )}
            </span>
          </div>
        );
      })}
    </div>
  );
}
