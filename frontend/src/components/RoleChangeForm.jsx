import { useState } from 'react';
import { changeRole } from '../api/userApi';
import '../shared/styles/forms.css';

const ROLES = ['Admin', 'ProjectManager', 'TeamMember', 'Client'];

export default function RoleChangeForm({ user, currentAdminId, token, onCancel, onSaved }) {
  const [role, setRole] = useState(user.role);
  const [submitting, setSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');

  const handleSubmit = async (event) => {
    event.preventDefault();
    if (role === user.role) {
      onCancel();
      return;
    }

    setSubmitting(true);
    setErrorMessage('');
    try {
      await changeRole(user.userId, role, currentAdminId, token);
      onSaved();
    } catch (error) {
      const httpStatus = error && error.status;
      setErrorMessage(
        httpStatus === 400
          ? "This change isn't allowed (you can't remove your own admin rights)."
          : httpStatus === 403
            ? "You don't have permission to change roles."
            : 'Something went wrong while changing the role. Please try again.'
      );
      setSubmitting(false);
    }
  };

  return (
    <form className="stacked-form" onSubmit={handleSubmit} noValidate>
      {errorMessage && (
        <p className="form-message error" role="alert">
          {errorMessage}
        </p>
      )}

      <p className="status-hint">
        Changing the role for <strong>{user.username}</strong>. They'll be signed out of all
        active sessions and will need to log in again for the new role to take effect.
      </p>

      <label>
        Role
        <select value={role} onChange={(event) => setRole(event.target.value)}>
          {ROLES.map((r) => (
            <option key={r} value={r}>
              {r}
            </option>
          ))}
        </select>
      </label>

      <div className="modal-actions">
        <button
          type="button"
          className="secondary-button"
          onClick={onCancel}
          disabled={submitting}
        >
          Cancel
        </button>
        <button type="submit" className="primary-button" disabled={submitting}>
          {submitting ? 'Saving…' : 'Save role'}
        </button>
      </div>
    </form>
  );
}
