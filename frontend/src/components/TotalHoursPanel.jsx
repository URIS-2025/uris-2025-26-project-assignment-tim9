import './TotalHoursPanel.css';

function formatHours(hours) {
  return Number.isInteger(hours) ? String(hours) : hours.toFixed(2).replace(/\.?0+$/, '');
}

/**
 * @param {object} props
 * @param {Array<{projectId: string, name: string}>} props.projects
 * @param {Array<{projectId: string, hoursSpent: number}>} props.timelogs - the current user's own timelogs
 */
export default function TotalHoursPanel({ projects, timelogs }) {
  const totalsByProject = new Map();
  for (const log of timelogs) {
    totalsByProject.set(log.projectId, (totalsByProject.get(log.projectId) || 0) + log.hoursSpent);
  }

  const grandTotal = timelogs.reduce((sum, log) => sum + log.hoursSpent, 0);

  const rows = projects.map((p) => ({
    projectId: p.projectId,
    name: p.name,
    hours: totalsByProject.get(p.projectId) || 0,
  }));

  return (
    <section className="total-hours-panel">
      <div className="total-hours-header">
        <h2>Total hours by project</h2>
        <div className="grand-total">
          <span className="grand-total-value">{formatHours(grandTotal)}</span>
          <span className="grand-total-label">hours logged</span>
        </div>
      </div>

      {rows.length === 0 ? (
        <p className="empty-hint">You are not on any projects yet.</p>
      ) : (
        <div className="project-hour-cards">
          {rows.map((row) => (
            <div key={row.projectId} className="project-hour-card">
              <span className="project-hour-name" title={row.name}>
                {row.name}
              </span>
              <span className="project-hour-value">
                {formatHours(row.hours)}
                <span className="unit">h</span>
              </span>
            </div>
          ))}
        </div>
      )}
    </section>
  );
}
