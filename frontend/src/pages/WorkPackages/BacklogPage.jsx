import { useParams, Link } from 'react-router-dom';
import BacklogView from '../../components/BacklogView';
import './BacklogPage.css';

export default function BacklogPage() {
  const { projectId } = useParams();

  return (
    <section className="backlog-page">
      <Link to={`/projects/${projectId}/work-packages`} className="backlog-page__back">
        ← Back to Work Packages
      </Link>

      <h1>Backlog</h1>

      <BacklogView projectId={projectId} />
    </section>
  );
}
