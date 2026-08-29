import { useState } from 'react';
import { createMilestone } from '../api/projectApi';
import '../shared/styles/forms.css';

function today() {
  return new Date().toISOString().slice(0, 10);
}

function tomorrow() {
  const date = new Date();
  date.setDate(date.getDate() + 1);
  return date.toISOString().slice(0, 10);
}

export default function MilestoneForm({ projectId, token, onCreated, onCancel }) {
  const [expectedDate, setExpectedDate] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');

  const dateError = !expectedDate
    ? 'Expected date is required.'
    : expectedDate <= today()
      ? 'Expected date must be in the future.'
      : '';

  const handleSubmit = async (event) => {
    event.preventDefault();
    if (dateError) {
      setErrorMessage('Please fix the highlighted field.');
      return;
    }

    setSubmitting(true);
    setErrorMessage('');
    try {
      await createMilestone({ projectId, expectedDate }, token);
      onCreated();
    } catch (error) {
      setErrorMessage(
        error && error.status === 400
          ? error.message || 'The milestone is invalid. Please pick a future date.'
          : 'Something went wrong while adding the milestone. Please try again.'
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

      <label>
        Expected Date
        <input
          type="date"
          value={expectedDate}
          min={tomorrow()}
          onChange={(event) => setExpectedDate(event.target.value)}
          required
        />
        {dateError && <span className="field-hint error">{dateError}</span>}
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
        <button
          type="submit"
          className="primary-button"
          disabled={submitting || Boolean(dateError)}
        >
          {submitting ? 'Adding…' : 'Add Milestone'}
        </button>
      </div>
    </form>
  );
}
