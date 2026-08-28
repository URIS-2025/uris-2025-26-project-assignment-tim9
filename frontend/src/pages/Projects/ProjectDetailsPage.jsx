import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useAuth } from '../../auth/useAuth';
import { getProjectById } from '../../api/projectApi';
import ProjectStatusBadge from '../../components/ProjectStatusBadge';
import ProjectsState from '../../components/ProjectsState';
import MilestoneList from '../../components/MilestoneList';
import RequirementList from '../../components/RequirementList';
// ProjectsState styles live in ProjectListPage.css
import './ProjectListPage.css';
import './ProjectDetailsPage.css';

const numberFormat = new Intl.NumberFormat();
const dateFormat = new Intl.DateTimeFormat(undefined, {
  day: 'numeric',
  month: 'short',
  year: 'numeric',
});
const dateTimeFormat = new Intl.DateTimeFormat(undefined, {
  day: 'numeric',
  month: 'short',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
});

function formatDate(value, formatter) {
  if (!value) return null;
  const parsed = new Date(value);
  return Number.isNaN(parsed.getTime()) ? null : formatter.format(parsed);
}

function DetailRow({ label, children, muted = false }) {
  return (
    <div className="pd-row">
      <dt className="pd-row__label">{label}</dt>
      <dd className={muted ? 'pd-row__value is-muted' : 'pd-row__value'}>{children}</dd>
    </div>
  );
}

function ProjectDetailsSkeleton() {
  return (
    <article className="pd-card pd-card--skeleton" aria-hidden="true">
      <dl className="pd-rows">
        {Array.from({ length: 4 }).map((_, index) => (
          <div className="pd-row" key={index}>
            <span className="pd-skeleton pd-skeleton--label" />
            <span className="pd-skeleton pd-skeleton--line" />
          </div>
        ))}
      </dl>
    </article>
  );
}

export default function ProjectDetailsPage() {
  const { id } = useParams();
  const { token } = useAuth();
  const [project, setProject] = useState(null);
  // phase: 'loading' | 'ready' | 'notfound' | 'error'
  const [phase, setPhase] = useState('loading');
  const [errorMessage, setErrorMessage] = useState('');
  const [reloadKey, setReloadKey] = useState(0);

  useEffect(() => {
    let ignore = false;

    getProjectById(id, token)
      .then((data) => {
        if (ignore) return;
        if (!data) {
          setPhase('notfound');
          return;
        }
        setProject(data);
        setErrorMessage('');
        setPhase('ready');
      })
      .catch((error) => {
        if (ignore) return;
        if (error && error.status === 404) {
          setPhase('notfound');
          return;
        }
        setErrorMessage(
          error && error.status === 401
            ? 'Your session has expired. Please sign in again to see this project.'
            : 'Something went wrong while loading the project. Check your connection and try again.'
        );
        setPhase('error');
      });

    return () => {
      ignore = true;
    };
  }, [id, token, reloadKey]);

  const reload = () => {
    setPhase('loading');
    setErrorMessage('');
    setReloadKey((key) => key + 1);
  };

  const deadline = project && formatDate(project.deadline, dateFormat);
  const createdAt = project && formatDate(project.createdAt, dateTimeFormat);

  return (
    <section className="project-details-page">
      <header className="pd-header">
        <Link to="/projects" className="pd-back">
          <span aria-hidden="true">&larr;</span> Projects
        </Link>
        <h1 className="pd-title">{phase === 'ready' ? project.name : 'Project details'}</h1>
      </header>

      {phase === 'loading' && (
        <>
          <p className="pd-visually-hidden" role="status">
            Loading project
          </p>
          <ProjectDetailsSkeleton />
        </>
      )}

      {phase === 'error' && (
        <ProjectsState variant="error" title={'We couldn’t load the project'} onRetry={reload}>
          {errorMessage}
        </ProjectsState>
      )}

      {phase === 'notfound' && (
        <ProjectsState variant="empty" title="Project not found">
          We couldn’t find a project with that id. It may have been removed.
        </ProjectsState>
      )}

      {phase === 'ready' && (
        <>
          <article className="pd-card">
            <dl className="pd-rows">
              <DetailRow label="Status">
                <ProjectStatusBadge status={project.status} />
              </DetailRow>
              <DetailRow label="Budget">
                <span className="pd-budget">{numberFormat.format(project.budget ?? 0)}</span>
              </DetailRow>
              <DetailRow label="Deadline" muted={!deadline}>
                {deadline ?? 'Not set'}
              </DetailRow>
              <DetailRow label="Created" muted={!createdAt}>
                {createdAt ?? 'Unknown'}
              </DetailRow>
            </dl>
          </article>

          <MilestoneList projectId={project.projectId} token={token} />
          <RequirementList projectId={project.projectId} token={token} />
        </>
      )}
    </section>
  );
}
