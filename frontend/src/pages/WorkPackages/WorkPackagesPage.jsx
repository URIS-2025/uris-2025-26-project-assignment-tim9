import { useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useAuth } from '../../auth/useAuth';
import { createWorkPackage } from '../../api/workPackageApi';
import WorkPackageList from '../../components/WorkPackageList';
import Modal from '../../components/Modal';
import './WorkPackagesPage.css';

export default function WorkPackagesPage() {
  const { projectId } = useParams();
  const { token } = useAuth();
  const [showCreate, setShowCreate] = useState(false);
  const [refreshKey, setRefreshKey] = useState(0);

  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [deadline, setDeadline] = useState('');
  const [saving, setSaving] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');

  function openCreate() {
    setName('');
    setDescription('');
    setDeadline('');
    setErrorMessage('');
    setShowCreate(true);
  }

  function closeCreate() {
    if (saving) return;
    setShowCreate(false);
  }

  async function handleCreateSubmit(event) {
    event.preventDefault();
    if (!name.trim()) return;
    setSaving(true);
    setErrorMessage('');
    try {
      await createWorkPackage(projectId, { name: name.trim(), description, deadline }, token);
      setShowCreate(false);
      setRefreshKey((key) => key + 1);
    } catch (error) {
      setErrorMessage(error?.message || 'Could not create the work package.');
    } finally {
      setSaving(false);
    }
  }

  return (
    <section className="work-packages-page">
      <Link to="/projects" className="work-packages-page__back">
        ← Back to Projects
      </Link>

      <header className="work-packages-page__header">
        <h1>Work Packages</h1>
        <Link to={`/projects/${projectId}/backlog`} className="work-packages-page__backlog-link">
          Backlog →
        </Link>
      </header>

      <WorkPackageList key={refreshKey} projectId={projectId} onCreateClick={openCreate} />

      {showCreate && (
        <Modal title="New Work Package" onClose={closeCreate}>
          <form className="wp-create-form" onSubmit={handleCreateSubmit}>
            <label>
              Name
              <input
                type="text"
                name="name"
                placeholder="Work package name"
                value={name}
                onChange={(event) => setName(event.target.value)}
                required
              />
            </label>
            <label>
              Description
              <textarea
                name="description"
                rows={3}
                placeholder="Short description"
                value={description}
                onChange={(event) => setDescription(event.target.value)}
              />
            </label>
            <label>
              Deadline
              <input
                type="date"
                name="deadline"
                value={deadline}
                onChange={(event) => setDeadline(event.target.value)}
                required
              />
            </label>

            {errorMessage && <p className="wp-create-form__error">{errorMessage}</p>}

            <button type="submit" disabled={saving}>
              {saving ? 'Saving...' : 'Save'}
            </button>
          </form>
        </Modal>
      )}
    </section>
  );
}
