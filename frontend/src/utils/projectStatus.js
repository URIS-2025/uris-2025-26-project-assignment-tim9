// Shared status logic for the Projects pages: maps a ProjectStatus (numeric
// index or string) to a display label and a colour "tone" used by the CSS.
//
// ProjectService serialises ProjectStatus as its numeric index (no
// JsonStringEnumConverter is registered), but we still accept the string
// form so this keeps working if that ever changes.
export const STATUS_ORDER = ['Planned', 'Active', 'OnHold', 'Completed', 'Cancelled'];

export const STATUS_META = {
  Planned: { label: 'Planned', tone: 'neutral' },
  Active: { label: 'Active', tone: 'in-progress' },
  OnHold: { label: 'On Hold', tone: 'critical' },
  Completed: { label: 'Completed', tone: 'done' },
  Cancelled: { label: 'Cancelled', tone: 'critical' },
};

export function resolveStatus(status) {
  const key = typeof status === 'number' ? STATUS_ORDER[status] : status;
  const meta = STATUS_META[key];
  if (meta) return { key, ...meta };
  return { key, label: status == null ? 'Unknown' : String(status), tone: 'neutral' };
}
