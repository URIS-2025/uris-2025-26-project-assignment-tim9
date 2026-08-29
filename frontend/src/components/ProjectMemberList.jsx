import { useEffect, useState } from 'react';
import { getProjectMembersByProjectId } from '../api/projectApi';
import './ProjectMemberList.css';

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

function ProjectMemberListSkeleton() {
  return (
    <ul className="member-list__items" aria-hidden="true">
      {Array.from({ length: 3 }).map((_, index) => (
        <li className="member-row member-row--skeleton" key={index}>
          <span className="pm-skeleton pm-skeleton--name" />
          <span className="pm-skeleton pm-skeleton--date" />
        </li>
      ))}
    </ul>
  );
}

export default function ProjectMemberList({ projectId, token }) {
  const [members, setMembers] = useState([]);
  // phase: 'loading' | 'ready' | 'error'
  const [phase, setPhase] = useState('loading');
  const [reloadKey, setReloadKey] = useState(0);

  useEffect(() => {
    if (!projectId) return undefined;
    let ignore = false;

    getProjectMembersByProjectId(projectId, token)
      .then((data) => {
        if (ignore) return;
        setMembers(Array.isArray(data) ? data : []);
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
    <section className="member-list">
      <h2 className="member-list__title">Members</h2>

      {phase === 'loading' && <ProjectMemberListSkeleton />}

      {phase === 'error' && (
        <div className="member-list__state member-list__state--error" role="alert">
          <p>We couldn’t load the members.</p>
          <button type="button" className="member-list__retry" onClick={reload}>
            Try again
          </button>
        </div>
      )}

      {phase === 'ready' && members.length === 0 && (
        <p className="member-list__state member-list__state--empty">No members yet.</p>
      )}

      {phase === 'ready' && members.length > 0 && (
        <ul className="member-list__items">
          {members.map((member, index) => {
            const joined = formatDate(member.joinedAt);
            return (
              <li className="member-row" key={member.projectMemberId ?? index}>
                <span className="member-row__id">
                  <span className="member-row__name">{member.username || member.userId}</span>
                  <span className="member-row__role">{member.role || 'Unknown role'}</span>
                </span>
                <span className="member-row__joined">
                  {joined ? `Joined ${joined}` : 'Join date unknown'}
                </span>
              </li>
            );
          })}
        </ul>
      )}
    </section>
  );
}
