import { useState } from 'react';
import { createProject } from '../api/projectApi';
import '../shared/styles/forms.css';

// ProjectStatus.Planned (ProjectStatus.cs: Planned, Active, OnHold, Completed, Cancelled)
const DEFAULT_STATUS = 0;

function today() {
  return new Date().toISOString().slice(0, 10);
}

export default function ProjectForm({ token, onCreated, onCancel }) {
  const [name, setName] = useState('');
  const [budget, setBudget] = useState('');
  const [deadline, setDeadline] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');

  const trimmedName = name.trim();
  const budgetNumber = Number(budget);
  const nameError = trimmedName ? '' : 'Name is required.';
  const budgetError =
    budget === ''
      ? 'Budget is required.'
      : Number.isNaN(budgetNumber)
        ? 'Budget must be a number.'
        : budgetNumber < 0
          ? 'Budget must not be negative.'
          : '';

  const handleSubmit = async (event) => {
    event.preventDefault();
    if (nameError || budgetError) {
      setErrorMessage('Please fix the highlighted fields.');
      return;
    }

    setSubmitting(true);
    setErrorMessage('');
    try {
      await createProject(
        {
          name: trimmedName,
          budget: budgetNumber,
          status: DEFAULT_STATUS,
          deadline: deadline || null,
        },
        token
      );
      onCreated();
    } catch (error) {
      setErrorMessage(
        error && error.status === 400
          ? error.message || 'Some fields are invalid. Please check your input.'
          : 'Something went wrong while creating the project. Please try again.'
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
        Name
        <input
          type="text"
          value={name}
          onChange={(event) => setName(event.target.value)}
          placeholder="e.g. Website redesign"
          maxLength={200}
          required
        />
        {nameError && <span className="field-hint error">{nameError}</span>}
      </label>

      <label>
        Budget
        <input
          type="number"
          value={budget}
          onChange={(event) => setBudget(event.target.value)}
          placeholder="0"
          min="0"
          step="1"
          required
        />
        {budgetError && <span className="field-hint error">{budgetError}</span>}
      </label>

      <label>
        Deadline (optional)
        <input
          type="date"
          value={deadline}
          min={today()}
          onChange={(event) => setDeadline(event.target.value)}
        />
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
          disabled={submitting || Boolean(nameError) || Boolean(budgetError)}
        >
          {submitting ? 'Creating…' : 'Create Project'}
        </button>
      </div>
    </form>
  );
}
