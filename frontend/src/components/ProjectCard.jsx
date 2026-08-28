import ProjectStatusBadge from './ProjectStatusBadge';
import { resolveStatus } from '../utils/projectStatus';

const numberFormat = new Intl.NumberFormat();
const dateFormat = new Intl.DateTimeFormat(undefined, {
  day: 'numeric',
  month: 'short',
  year: 'numeric',
});

function formatDeadline(value) {
  if (!value) return null;
  const parsed = new Date(value);
  return Number.isNaN(parsed.getTime()) ? null : dateFormat.format(parsed);
}

function isOverdue(value, statusKey) {
  if (!value || statusKey === 'Completed' || statusKey === 'Cancelled') return false;
  const parsed = new Date(value);
  return !Number.isNaN(parsed.getTime()) && parsed.getTime() < Date.now();
}

// A single project card: name + status badge, then budget and deadline.
export default function ProjectCard({ project }) {
  const status = resolveStatus(project.status);
  const deadline = formatDeadline(project.deadline);
  const overdue = isOverdue(project.deadline, status.key);

  return (
    <article className="project-card">
      <div className="project-card__head">
        <h2 className="project-card__name">{project.name}</h2>
        <ProjectStatusBadge status={project.status} />
      </div>

      <dl className="project-card__meta">
        <div className="project-meta">
          <dt>Budget</dt>
          <dd>
            <span className="project-meta__budget">
              {numberFormat.format(project.budget ?? 0)}
            </span>
          </dd>
        </div>
        <div className="project-meta">
          <dt>Deadline</dt>
          <dd className={deadline ? undefined : 'is-muted'}>
            <span>{deadline ?? 'Not set'}</span>
            {overdue && <span className="project-meta__overdue">Overdue</span>}
          </dd>
        </div>
      </dl>
    </article>
  );
}
