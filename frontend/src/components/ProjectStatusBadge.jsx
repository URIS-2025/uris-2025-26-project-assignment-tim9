import { resolveStatus } from '../utils/projectStatus';

export default function ProjectStatusBadge({ status }) {
  const { label, tone } = resolveStatus(status);
  return <span className={`status-pill status-pill--${tone}`}>{label}</span>;
}
