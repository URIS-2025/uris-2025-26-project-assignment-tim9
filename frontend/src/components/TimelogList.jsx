import './TimelogList.css';

function formatDate(iso) {
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return iso;
  return d.toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
}

/**
 * @param {object} props
 * @param {Array} props.timelogs - enriched with projectName / taskTitle, newest first
 * @param {(timelog: object) => void} props.onEdit
 * @param {(id: string) => void} props.onDelete
 * @param {string|null} props.deletingId
 */
export default function TimelogList({ timelogs, onEdit, onDelete, deletingId }) {
  if (timelogs.length === 0) {
    return (
      <div className="timelog-list-empty">
        You haven't logged any time yet. Use the + button above to add your first entry.
      </div>
    );
  }

  return (
    <div className="timelog-list">
      <div className="timelog-row timelog-row-head">
        <span>Task</span>
        <span>Project</span>
        <span>Date</span>
        <span className="align-right">Hours</span>
        <span className="align-right">Actions</span>
      </div>
      {timelogs.map((log) => (
        <div className="timelog-row" key={log.id}>
          <span className="timelog-task" title={log.taskTitle}>
            {log.taskTitle}
          </span>
          <span className="timelog-project" title={log.projectName}>
            {log.projectName}
          </span>
          <span className="timelog-date">{formatDate(log.date)}</span>
          <span className="timelog-hours align-right">{log.hoursSpent}h</span>
          <span className="timelog-actions align-right">
            <button type="button" className="icon-button" onClick={() => onEdit(log)}>
              Update
            </button>
            <button
              type="button"
              className="icon-button danger"
              disabled={deletingId === log.id}
              onClick={() => onDelete(log.id)}
            >
              {deletingId === log.id ? 'Deleting…' : 'Delete'}
            </button>
          </span>
        </div>
      ))}
    </div>
  );
}
