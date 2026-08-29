import { useState } from 'react';
import Modal from './Modal';
import { createIntegration, updateIntegration } from '../api/integrationApi';

/**
 * Creates or edits an integration via IntegrationService.
 *
 * Edit mode: the API key field is left blank and is optional - submitting it
 * blank keeps the existing (encrypted) key in place, filling it in rotates it.
 * This mirrors IntegrationUpdateDTO, which only rotates the key when ApiKey
 * is non-empty.
 *
 * @param {object} [props.integration] - the integration being edited; omit to create instead
 * @param {() => void} props.onClose
 * @param {() => void} props.onSaved
 */
export default function IntegrationFormModal({ integration, token, onClose, onSaved }) {
  const isEditing = Boolean(integration);

  const [type, setType] = useState(integration?.type ?? '');
  const [apiKey, setApiKey] = useState('');
  const [status, setStatus] = useState(integration?.status ?? true);

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);

  async function handleSubmit(e) {
    e.preventDefault();
    setError(null);

    if (!type.trim()) {
      setError('Give the integration a type.');
      return;
    }
    if (!isEditing && apiKey.trim().length < 8) {
      setError('API key must be at least 8 characters.');
      return;
    }
    if (isEditing && apiKey.trim() && apiKey.trim().length < 8) {
      setError('API key must be at least 8 characters.');
      return;
    }

    setSubmitting(true);
    try {
      if (isEditing) {
        await updateIntegration(
          integration.id,
          { type: type.trim(), apiKey: apiKey.trim() || null, status },
          token
        );
      } else {
        await createIntegration({ type: type.trim(), apiKey: apiKey.trim() }, token);
      }
      onSaved();
    } catch (err) {
      setError(err.message || `Could not ${isEditing ? 'save' : 'create'} the integration.`);
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <Modal title={isEditing ? 'Edit integration' : 'New integration'} onClose={onClose}>
      <form className="stacked-form" onSubmit={handleSubmit}>
        {error && <div className="form-message error">{error}</div>}

        <label>
          Type
          <input
            required
            placeholder="e.g. Slack, GitHub, Stripe"
            value={type}
            onChange={(e) => setType(e.target.value)}
          />
        </label>

        <label>
          API key {isEditing && <span className="field-hint">(leave blank to keep the current one)</span>}
          <input
            type="password"
            autoComplete="new-password"
            placeholder={isEditing ? '••••••••' : ''}
            value={apiKey}
            onChange={(e) => setApiKey(e.target.value)}
          />
        </label>

        {isEditing && (
          <label>
            Status
            <select value={String(status)} onChange={(e) => setStatus(e.target.value === 'true')}>
              <option value="true">Active</option>
              <option value="false">Inactive</option>
            </select>
          </label>
        )}

        <div className="modal-actions">
          <button type="button" className="secondary-button" onClick={onClose}>
            Cancel
          </button>
          <button type="submit" className="primary-button" disabled={submitting}>
            {submitting
              ? isEditing
                ? 'Saving…'
                : 'Creating…'
              : isEditing
                ? 'Save changes'
                : 'Create integration'}
          </button>
        </div>
      </form>
    </Modal>
  );
}
