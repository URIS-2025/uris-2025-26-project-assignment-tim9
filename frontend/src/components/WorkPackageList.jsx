import { useEffect, useState } from 'react';
//import { getWorkPackages } from '../api/workPackageApi';

const MOCK_WORK_PACKAGES = [
  { id: '1', name: 'Autentifikacija i autorizacija', description: 'JWT middleware i role-based auth', status: 'InProgress' },
  { id: '2', name: 'Upravljanje zadacima', description: 'CRUD za Task entitet sa sub-taskovima', status: 'Done' },
  { id: '3', name: 'Notifikacije', description: 'Integracija sa NotificationService', status: 'ToDo' },
];

const USE_MOCK_DATA = true; // promeni u false kad backend proradi

export default function WorkPackageList({ projectId }) {
  const [workPackages, setWorkPackages] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    if (USE_MOCK_DATA) {
      setWorkPackages(MOCK_WORK_PACKAGES);
      setLoading(false);
      return;
    }

    getWorkPackages(projectId)
      .then(setWorkPackages)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, [projectId]);

  if (loading) return <p>Učitavanje...</p>;
  if (error) return <p style={{ color: 'var(--color-status-critical)' }}>{error}</p>;

  return (
    <div>
      <h2>Work Package-i</h2>
      <ul style={{ listStyle: 'none', padding: 0, maxWidth: '600px', margin: '0 auto' }}>
        {workPackages.map((wp) => (
          <li
            key={wp.id}
            style={{
              border: '1px solid var(--border)',
              borderRadius: '8px',
              padding: '12px 16px',
              marginBottom: '8px',
              textAlign: 'left',
            }}
          >
            <strong>{wp.name}</strong>
            <p style={{ margin: '4px 0 0', color: 'var(--text)' }}>{wp.description}</p>
          </li>
        ))}
      </ul>
    </div>
  );
}