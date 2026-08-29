import { useState } from 'react';
import { createRequirement, updateRequirement } from '../api/projectApi';
import '../shared/styles/forms.css';

const MAX_LENGTH = 2000;

export default function RequirementForm({
  mode = 'create',
  requirement = null,
  projectId,
  token,
  onCreated,
  onSaved,
  onCancel,
}) {
  const isEdit = mode === 'edit';
  const [description, setDescription] = useState(requirement?.description ?? '');
  const [submitting, setSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');

  const trimmed = description.trim();
  const descriptionError = !trimmed
    ? 'Description is required.'
    : description.length > MAX_LENGTH
      ? `Description must be ${MAX_LENGTH} characters or fewer.`
      : '';

  const handleSubmit = async (event) => {
    event.preventDefault();
    if (descriptionError) {
      setErrorMessage('Please fix the highlighted field.');
      return;
    }

    setSubmitting(true);
    setErrorMessage('');
    try {
      if (isEdit) {
        await updateRequirement(
          { requirementId: requirement.requirementId, projectId, description: trimmed },
          token
        );
      } else {
        await createRequirement({ projectId, description: trimmed }, token);
      }
      (onSaved || onCreated)();
    } catch (error) {
      const httpStatus = error && error.status;
      setErrorMessage(
        httpStatus === 403
          ? `You don't have permission to ${isEdit ? 'edit' : 'add'} requirements.`
          : httpStatus === 400
            ? error.message || 'The requirement is invalid. Please check the description.'
            : `Something went wrong while ${isEdit ? 'saving' : 'adding'} the requirement. Please try again.`
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
        Description
        <textarea
          value={description}
          onChange={(event) => setDescription(event.target.value)}
          placeholder="Describe what the project must deliver"
          rows={5}
          maxLength={MAX_LENGTH}
          required
        />
        <span className={descriptionError ? 'field-hint error' : 'field-hint status-hint'}>
          {descriptionError || `${description.length} / ${MAX_LENGTH}`}
        </span>
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
          disabled={submitting || Boolean(descriptionError)}
        >
          {submitting
            ? isEdit
              ? 'Saving…'
              : 'Adding…'
            : isEdit
              ? 'Save Requirement'
              : 'Add Requirement'}
        </button>
      </div>
    </form>
  );
}
