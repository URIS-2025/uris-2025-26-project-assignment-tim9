import { useState } from 'react';
import { createProject, updateProject } from '../api/projectApi';
import { STATUS_ORDER, STATUS_META } from '../utils/projectStatus';
import '../shared/styles/forms.css';

const DEFAULT_STATUS = 0;

function today() {
  return new Date().toISOString().slice(0, 10);
}

function toDateInput(value) {
  return value ? String(value).slice(0, 10) : '';
}

export default function ProjectForm({
  mode = 'create',
  project = null,
  token,
  onCreated,
  onSaved,
  onCancel,
}) {
  const isEdit = mode === 'edit';
  const [name, setName] = useState(project?.name ?? '');
  const [budget, setBudget] = useState(project ? String(project.budget ?? '') : '');
  const [status, setStatus] = useState(String(project?.status ?? DEFAULT_STATUS));
  const [deadline, setDeadline] = useState(toDateInput(project?.deadline));
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
      if (isEdit) {
        await updateProject(
          {
            projectId: project.projectId,
            name: trimmedName,
            budget: budgetNumber,
            status: Number(status),
            deadline: deadline || null,
          },
          token
        );
      } else {
        await createProject(
          {
            name: trimmedName,
            budget: budgetNumber,
            status: DEFAULT_STATUS,
            deadline: deadline || null,
          },
          token
        );
      }
      (onSaved || onCreated)();
    } catch (error) {
      const httpStatus = error && error.status;
      setErrorMessage(
        httpStatus === 403
          ? `You don't have permission to ${isEdit ? 'edit' : 'create'} this project.`
          : httpStatus === 400
            ? error.message || 'Some fields are invalid. Please check your input.'
            : `Something went wrong while ${isEdit ? 'saving' : 'creating'} the project. Please try again.`
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

      {isEdit && (
        <label>
          Status
          <select value={status} onChange={(event) => setStatus(event.target.value)}>
            {STATUS_ORDER.map((key, index) => (
              <option key={key} value={index}>
                {STATUS_META[key].label}
              </option>
            ))}
          </select>
        </label>
      )}

      <label>
        Deadline (optional)
        <input
          type="date"
          value={deadline}
          min={isEdit ? undefined : today()}
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
          {submitting
            ? isEdit
              ? 'Saving…'
              : 'Creating…'
            : isEdit
              ? 'Edit Project'
              : 'Create Project'}
        </button>
      </div>
    </form>
  );
}
