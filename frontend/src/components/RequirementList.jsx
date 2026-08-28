import { useEffect, useState } from 'react';
import { getRequirementsByProjectId } from '../api/projectApi';
import './RequirementList.css';

function RequirementListSkeleton() {
  return (
    <ul className="requirement-list__items" aria-hidden="true">
      {Array.from({ length: 3 }).map((_, index) => (
        <li className="requirement-row requirement-row--skeleton" key={index}>
          <span className="rl-skeleton rl-skeleton--line" />
          <span className="rl-skeleton rl-skeleton--line is-short" />
        </li>
      ))}
    </ul>
  );
}

export default function RequirementList({ projectId, token }) {
  const [requirements, setRequirements] = useState([]);
  // phase: 'loading' | 'ready' | 'error'
  const [phase, setPhase] = useState('loading');
  const [reloadKey, setReloadKey] = useState(0);

  useEffect(() => {
    if (!projectId) return undefined;
    let ignore = false;

    getRequirementsByProjectId(projectId, token)
      .then((data) => {
        if (ignore) return;
        setRequirements(Array.isArray(data) ? data : []);
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
    <section className="requirement-list">
      <h2 className="requirement-list__title">Requirements</h2>

      {phase === 'loading' && <RequirementListSkeleton />}

      {phase === 'error' && (
        <div className="requirement-list__state requirement-list__state--error" role="alert">
          <p>We couldn’t load the requirements.</p>
          <button type="button" className="requirement-list__retry" onClick={reload}>
            Try again
          </button>
        </div>
      )}

      {phase === 'ready' && requirements.length === 0 && (
        <p className="requirement-list__state requirement-list__state--empty">
          No requirements yet.
        </p>
      )}

      {phase === 'ready' && requirements.length > 0 && (
        <ul className="requirement-list__items">
          {requirements.map((requirement, index) => (
            <li className="requirement-row" key={requirement.requirementId ?? index}>
              <span className="requirement-row__text">
                {requirement.description || 'No description'}
              </span>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
