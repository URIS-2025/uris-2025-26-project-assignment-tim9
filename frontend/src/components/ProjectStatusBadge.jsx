import { resolveStatus } from '../utils/projectStatus';

// Coloured status pill for a project. Accepts the raw ProjectStatus value
// (numeric index or string) and resolves it internally, so callers can pass
// `project.status` straight through.
export default function ProjectStatusBadge({ status }) {
  const { label, tone } = resolveStatus(status);
  return <span className={`status-pill status-pill--${tone}`}>{label}</span>;
}
