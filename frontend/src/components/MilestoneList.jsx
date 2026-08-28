import { useEffect, useState } from 'react';
import { getMilestonesByProjectId } from '../api/projectApi';
import './MilestoneList.css';

const dateFormat = new Intl.DateTimeFormat(undefined, {
  day: 'numeric',
  month: 'short',
  year: 'numeric',
});

function formatDate(value) {
  if (!value) return null;
  const parsed = new Date(value);
  return Number.isNaN(parsed.getTime()) ? null : dateFormat.format(parsed);
}

// past date -> Overdue, otherwise Upcoming
function resolveMilestoneState(expectedDate) {
  const parsed = new Date(expectedDate);
  if (Number.isNaN(parsed.getTime())) return { label: 'Unknown', tone: 'neutral' };
  return parsed.getTime() < Date.now()
    ? { label: 'Overdue', tone: 'critical' }
    : { label: 'Upcoming', tone: 'accent' };
}

function MilestoneListSkeleton() {
  return (
    <ul className="milestone-list__items" aria-hidden="true">
      {Array.from({ length: 3 }).map((_, index) => (
        <li className="milestone-row milestone-row--skeleton" key={index}>
          <span className="ml-skeleton ml-skeleton--date" />
          <span className="ml-skeleton ml-skeleton--pill" />
        </li>
      ))}
    </ul>
  );
}

export default function MilestoneList({ projectId, token }) {
  const [milestones, setMilestones] = useState([]);
  // phase: 'loading' | 'ready' | 'error'
  const [phase, setPhase] = useState('loading');
  const [reloadKey, setReloadKey] = useState(0);

  useEffect(() => {
    if (!projectId) return undefined;
    let ignore = false;

    getMilestonesByProjectId(projectId, token)
      .then((data) => {
        if (ignore) return;
        const list = Array.isArray(data) ? data : [];
        list.sort((a, b) => new Date(a.expectedDate) - new Date(b.expectedDate));
        setMilestones(list);
        setPhase('ready');
      })
      .catch(() => {
        if (ignore) return;
        setPhase('error');
      });

    return () => {
      ignore = true;
    };
  }, [projectId, token, reloadKey]);

  const reload = () => {
    setPhase('loading');
    setReloadKey((key) => key + 1);
  };

  return (
    <section className="milestone-list">
      <h2 className="milestone-list__title">Milestones</h2>

      {phase === 'loading' && <MilestoneListSkeleton />}

      {phase === 'error' && (
        <div className="milestone-list__state milestone-list__state--error" role="alert">
          <p>We couldn’t load the milestones.</p>
          <button type="button" className="milestone-list__retry" onClick={reload}>
            Try again
          </button>
        </div>
      )}

      {phase === 'ready' && milestones.length === 0 && (
        <p className="milestone-list__state milestone-list__state--empty">No milestones yet.</p>
      )}

      {phase === 'ready' && milestones.length > 0 && (
        <ol className="milestone-list__items">
          {milestones.map((milestone, index) => {
            const state = resolveMilestoneState(milestone.expectedDate);
            const date = formatDate(milestone.expectedDate);
            return (
              <li className="milestone-row" key={milestone.milestoneId ?? index}>
                <span className="milestone-row__date">{date ?? 'Unknown date'}</span>
                <span className={`milestone-pill milestone-pill--${state.tone}`}>
                  {state.label}
                </span>
              </li>
            );
          })}
        </ol>
      )}
    </section>
  );
}
