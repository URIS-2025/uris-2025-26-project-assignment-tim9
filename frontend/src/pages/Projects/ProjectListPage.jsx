import { useEffect, useState } from 'react';
import { useAuth } from '../../auth/useAuth';
import { getProjects } from '../../api/projectApi';
import ProjectCard from '../../components/ProjectCard';
import ProjectListSkeleton from '../../components/ProjectListSkeleton';
import ProjectsState from '../../components/ProjectsState';
import './ProjectListPage.css';

export default function ProjectListPage() {
  const { token } = useAuth();
  const [projects, setProjects] = useState([]);
  const [phase, setPhase] = useState('loading');
  const [errorMessage, setErrorMessage] = useState('');
  const [reloadKey, setReloadKey] = useState(0);

  useEffect(() => {
    let ignore = false;

    getProjects(token)
      .then((data) => {
        if (ignore) return;
        setProjects(Array.isArray(data) ? data : []);
        setErrorMessage('');
        setPhase('ready');
      })
      .catch((error) => {
        if (ignore) return;
        setErrorMessage(
          error && error.status === 401
            ? 'Your session has expired. Please sign in again to see the projects.'
            : 'Something went wrong while loading the projects. Check your connection and try again.'
        );
        setPhase('error');
      });

    return () => {
      ignore = true;
    };
  }, [token, reloadKey]);

  const reload = () => {
    setPhase('loading');
    setErrorMessage('');
    setReloadKey((key) => key + 1);
  };

  return (
    <section className="projects-page">
      <header className="projects-header">
        <div className="projects-title-row">
          <h1 className="projects-title">Projects</h1>
          {phase === 'ready' && projects.length > 0 && (
            <span className="projects-count">{projects.length}</span>
          )}
        </div>
        <p className="projects-subtitle">
          Every project at a glance &mdash; budget, current status and deadline.
        </p>
      </header>

      {phase === 'loading' && (
        <>
          <p className="pl-visually-hidden" role="status">
            Loading projects
          </p>
          <ProjectListSkeleton />
        </>
      )}

      {phase === 'error' && (
        <ProjectsState variant="error" title={'We couldn’t load the projects'} onRetry={reload}>
          {errorMessage}
        </ProjectsState>
      )}

      {phase === 'ready' && projects.length === 0 && (
        <ProjectsState variant="empty" title="No projects yet">
          When projects are created they&rsquo;ll appear here.
        </ProjectsState>
      )}

      {phase === 'ready' && projects.length > 0 && (
        <div className="projects-grid">
          {projects.map((project) => (
            <ProjectCard key={project.projectId ?? project.name} project={project} />
          ))}
        </div>
      )}
    </section>
  );
}
