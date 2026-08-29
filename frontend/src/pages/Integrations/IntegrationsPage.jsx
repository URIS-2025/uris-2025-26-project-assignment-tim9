import { useCallback, useEffect, useState } from 'react';
import { useAuth } from '../../auth/useAuth';
import { ApiError } from '../../api/httpClient';
import { getIntegrations, deleteIntegration } from '../../api/integrationApi';
import Modal from '../../components/Modal';
import IntegrationFormModal from '../../components/IntegrationFormModal';
import '../../components/listControls.css';
import '../../components/rowActions.css';
import './IntegrationsPage.css';

export default function IntegrationsPage() {
  const { token, logout } = useAuth();

  const [integrations, setIntegrations] = useState([]);
  const [phase, setPhase] = useState('loading');
  const [errorMessage, setErrorMessage] = useState('');
  const [reloadKey, setReloadKey] = useState(0);

  const [formState, setFormState] = useState(null); // null | { editing: integration|null }
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [deleting, setDeleting] = useState(false);
  const [deleteError, setDeleteError] = useState('');

  const handleAuthError = useCallback(
    (err) => {
      if (err instanceof ApiError && err.status === 401) {
        logout();
        return true;
      }
      return false;
    },
    [logout]
  );

  useEffect(() => {
    let cancelled = false;
    (async () => {
      setPhase('loading');
      setErrorMessage('');
      try {
        const data = await getIntegrations(token);
        if (cancelled) return;
        setIntegrations(data);
        setPhase('ready');
      } catch (err) {
        if (cancelled) return;
        if (!handleAuthError(err)) {
          setErrorMessage(
            err && err.status === 403
              ? "You don't have permission to view integrations."
              : 'Something went wrong while loading integrations. Check your connection and try again.'
          );
          setPhase('error');
        }
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [token, reloadKey, handleAuthError]);

  const reload = () => setReloadKey((k) => k + 1);

  function handleSaved() {
    setFormState(null);
    reload();
  }

  async function confirmDelete() {
    setDeleting(true);
    setDeleteError('');
    try {
      await deleteIntegration(deleteTarget.id, token);
      setDeleteTarget(null);
      reload();
    } catch (err) {
      if (!handleAuthError(err)) {
        setDeleteError(err.message || 'Could not delete this integration.');
      }
    } finally {
      setDeleting(false);
    }
  }

  return (
    <section className="integrations-page">
      <header className="page-header">
        <div>
          <h1>Integrations</h1>
          <p className="page-subtitle">Manage third-party API keys used by the platform.</p>
        </div>
      </header>

      <div className="list-toolbar">
        <button type="button" className="primary-button" onClick={() => setFormState({ editing: null })}>
          + New integration
        </button>
      </div>

      {phase === 'loading' && <p className="status-hint">Loading integrations…</p>}

      {phase === 'error' && (
        <div className="integrations-state integrations-state--error" role="alert">
          <p>{errorMessage}</p>
          <button type="button" className="secondary-button" onClick={reload}>
            Try again
          </button>
        </div>
      )}

      {phase === 'ready' && integrations.length === 0 && (
        <div className="integrations-state">
          <p>No integrations configured yet.</p>
        </div>
      )}

      {phase === 'ready' && integrations.length > 0 && (
        <div className="integrations-list">
          <div className="integrations-row integrations-row-head">
            <span>Type</span>
            <span>API key</span>
            <span>Status</span>
            <span>Created</span>
            <span className="align-right">Actions</span>
          </div>
          {integrations.map((integration) => (
            <div className="integrations-row" key={integration.id}>
              <span>{integration.type}</span>
              <span className="integrations-key">{integration.apiKeyMasked}</span>
              <span>
                <span
                  className={
                    integration.status ? 'status-badge status-badge--active' : 'status-badge'
                  }
                >
                  {integration.status ? 'Active' : 'Inactive'}
                </span>
              </span>
              <span className="integrations-date">
                {new Date(integration.createdAt).toLocaleDateString()}
              </span>
              <span className="row-actions align-right">
                <button
                  type="button"
                  className="row-action"
                  onClick={() => setFormState({ editing: integration })}
                >
                  Edit
                </button>
                <button
                  type="button"
                  className="row-action row-action--danger"
                  onClick={() => {
                    setDeleteError('');
                    setDeleteTarget(integration);
                  }}
                >
                  Delete
                </button>
              </span>
            </div>
          ))}
        </div>
      )}

      {formState && (
        <IntegrationFormModal
          integration={formState.editing}
          token={token}
          onClose={() => setFormState(null)}
          onSaved={handleSaved}
        />
      )}

      {deleteTarget && (
        <Modal title={`Delete ${deleteTarget.type}?`} onClose={() => setDeleteTarget(null)}>
          <p className="row-confirm-text">
            This permanently removes the integration and its stored API key. This can't be undone.
          </p>
          {deleteError && <p className="row-delete-error">{deleteError}</p>}
          <div className="modal-actions">
            <button type="button" className="secondary-button" onClick={() => setDeleteTarget(null)}>
              Cancel
            </button>
            <button
              type="button"
              className="row-delete-confirm"
              disabled={deleting}
              onClick={confirmDelete}
            >
              {deleting ? 'Deleting…' : 'Delete'}
            </button>
          </div>
        </Modal>
      )}
    </section>
  );
}
