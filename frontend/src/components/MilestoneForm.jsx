import { useState } from 'react';
import { createMilestone, updateMilestone } from '../api/projectApi';
import '../shared/styles/forms.css';

const NAME_MAX = 200;
const DESCRIPTION_MAX = 1000;

function today() {
  return new Date().toISOString().slice(0, 10);
}

function tomorrow() {
  const date = new Date();
  date.setDate(date.getDate() + 1);
  return date.toISOString().slice(0, 10);
}

function toDateInput(value) {
  return value ? String(value).slice(0, 10) : '';
}

export default function MilestoneForm({
  mode = 'create',
  milestone = null,
  projectId,
  token,
  onCreated,
  onSaved,
  onCancel,
}) {
  const isEdit = mode === 'edit';
  const [name, setName] = useState(milestone?.name ?? '');
  const [description, setDescription] = useState(milestone?.description ?? '');
  const [expectedDate, setExpectedDate] = useState(toDateInput(milestone?.expectedDate));
  const [submitting, setSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');

  const trimmedName = name.trim();
  const nameError = !trimmedName
    ? 'Name is required.'
    : name.length > NAME_MAX
      ? `Name must be ${NAME_MAX} characters or fewer.`
      : '';
  const dateError = !expectedDate
    ? 'Expected date is required.'
    : expectedDate <= today()
      ? 'Expected date must be in the future.'
      : '';

  const handleSubmit = async (event) => {
    event.preventDefault();
    if (nameError || dateError) {
      setErrorMessage('Please fix the highlighted fields.');
      return;
    }

    setSubmitting(true);
    setErrorMessage('');
    try {
      const payload = {
        projectId,
        name: trimmedName,
        description: description.trim() || null,
        expectedDate,
      };
      if (isEdit) {
        await updateMilestone({ ...payload, milestoneId: milestone.milestoneId }, token);
      } else {
        await createMilestone(payload, token);
      }
      (onSaved || onCreated)();
    } catch (error) {
      const httpStatus = error && error.status;
      setErrorMessage(
        httpStatus === 403
          ? `You don't have permission to ${isEdit ? 'edit' : 'add'} milestones.`
          : httpStatus === 400
            ? error.message || 'The milestone data is invalid. Please check the fields.'
            : `Something went wrong while ${isEdit ? 'saving' : 'adding'} the milestone. Please try again.`
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
          placeholder="e.g. Beta release"
          maxLength={NAME_MAX}
          required
        />
        {nameError && <span className="field-hint error">{nameError}</span>}
      </label>

      <label>
        Description (optional)
        <textarea
          value={description}
          onChange={(event) => setDescription(event.target.value)}
          placeholder="What this milestone covers"
          rows={3}
          maxLength={DESCRIPTION_MAX}
        />
      </label>

      <label>
        Expected Date
        <input
          type="date"
          value={expectedDate}
          min={isEdit ? undefined : tomorrow()}
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
          disabled={submitting || Boolean(nameError) || Boolean(dateError)}
        >
          {submitting
            ? isEdit
              ? 'Saving…'
              : 'Adding…'
            : isEdit
              ? 'Save Milestone'
              : 'Add Milestone'}
        </button>
      </div>
    </form>
  );
}
