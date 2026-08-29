import { useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import WorkPackageList from '../../components/WorkPackageList';
import Modal from '../../components/Modal';
import './WorkPackagesPage.css';

export default function WorkPackagesPage() {
  const { projectId } = useParams();
  const [showCreate, setShowCreate] = useState(false);

  function handleCreateSubmit(event) {
    event.preventDefault();
    // mock: za sad samo zatvaramo modal, ne šaljemo na backend
    setShowCreate(false);
  }

  return (
    <section className="work-packages-page">
      <header className="work-packages-page__header">
        <h1>Work Packages</h1>
        <Link to={`/projects/${projectId}/backlog`} className="work-packages-page__backlog-link">
          Backlog →
        </Link>
      </header>

      <WorkPackageList projectId={projectId} onCreateClick={() => setShowCreate(true)} />

      {showCreate && (
        <Modal title="New Work Package" onClose={() => setShowCreate(false)}>
          <form className="wp-create-form" onSubmit={handleCreateSubmit}>
            <label>
              Name
              <input type="text" name="name" placeholder="Work package name" required />
            </label>
            <label>
              Description
              <textarea name="description" rows={3} placeholder="Short description" />
            </label>
            <button type="submit">Save</button>
          </form>
        </Modal>
      )}
    </section>
  );
}
