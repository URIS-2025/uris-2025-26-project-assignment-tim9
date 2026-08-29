import { useEffect, useState } from 'react';
import { createProjectMember, updateProjectMember } from '../api/projectApi';
import { searchUsers } from '../api/userApi';
import '../shared/styles/forms.css';
import './ProjectMemberForm.css';

const MIN_QUERY = 2;

function displayName(user) {
  return user.name || user.username || user.userId;
}

function memberErrorMessage(error, verb) {
  const httpStatus = error && error.status;
  if (httpStatus === 403) return `You don't have permission to ${verb} members.`;
  if (httpStatus === 401) return 'Your session has expired. Please sign in again.';
  if (httpStatus === 400) return error.message || 'The request is invalid. Please try again.';
  return `Something went wrong while ${verb === 'edit' ? 'saving' : 'adding'} the member. Please try again.`;
}

export default function ProjectMemberForm({
  mode = 'create',
  member = null,
  projectId,
  token,
  onCreated,
  onSaved,
  onCancel,
}) {
  const isEdit = mode === 'edit';

  const [term, setTerm] = useState('');
  const [selected, setSelected] = useState(null);
  const [results, setResults] = useState([]);
  const [searching, setSearching] = useState(false);
  const [searchError, setSearchError] = useState('');
  const [status, setStatus] = useState(member ? member.status !== false : true);
  const [submitting, setSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');

  const query = term.trim();
  const showResults = !selected && query.length >= MIN_QUERY;

  useEffect(() => {
    if (isEdit) return undefined;
    if (selected) return undefined;
    if (query.length < MIN_QUERY) return undefined;

    let ignore = false;
    const timer = setTimeout(() => {
      searchUsers(query, token)
        .then((data) => {
          if (ignore) return;
          setResults(Array.isArray(data) ? data : []);
          setSearching(false);
          setSearchError('');
        })
        .catch(() => {
          if (ignore) return;
          setResults([]);
          setSearching(false);
          setSearchError('Could not search users. Please try again.');
        });
    }, 300);

    return () => {
      ignore = true;
      clearTimeout(timer);
    };
  }, [isEdit, query, token, selected]);

  const handleEditSubmit = async (event) => {
    event.preventDefault();
    setSubmitting(true);
    setErrorMessage('');
    try {
      await updateProjectMember(
        {
          projectMemberId: member.projectMemberId,
          projectId: member.projectId ?? projectId,
          userId: member.userId,
          joinedAt: member.joinedAt,
          status,
        },
        token
      );
      (onSaved || onCreated)();
    } catch (error) {
      setErrorMessage(memberErrorMessage(error, 'edit'));
      setSubmitting(false);
    }
  };

  const handleCreateSubmit = async (event) => {
    event.preventDefault();
    if (!selected) {
      setErrorMessage('Please pick a user from the search results.');
      return;
    }

    setSubmitting(true);
    setErrorMessage('');
    try {
      await createProjectMember({ projectId, userId: selected.userId }, token);
      (onSaved || onCreated)();
    } catch (error) {
      setErrorMessage(memberErrorMessage(error, 'add'));
      setSubmitting(false);
    }
  };

  if (isEdit) {
    return (
      <form className="stacked-form" onSubmit={handleEditSubmit} noValidate>
        {errorMessage && (
          <p className="form-message error" role="alert">
            {errorMessage}
          </p>
        )}

        <p className="user-search-selected">
          <strong>{member.username || member.userId}</strong>
          {member.role ? ` — ${member.role}` : ''}
        </p>

        <label>
          Status
          <select
            value={status ? 'active' : 'inactive'}
            onChange={(event) => setStatus(event.target.value === 'active')}
          >
            <option value="active">Active</option>
            <option value="inactive">Inactive</option>
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
            {submitting ? 'Saving…' : 'Save Member'}
          </button>
        </div>
      </form>
    );
  }

  const handleTermChange = (event) => {
    const value = event.target.value;
    setTerm(value);
    setSelected(null);
    setResults([]);
    setSearchError('');
    setSearching(value.trim().length >= MIN_QUERY);
  };

  const handleSelect = (user) => {
    setSelected(user);
    setTerm(displayName(user));
    setResults([]);
    setSearching(false);
    setSearchError('');
  };

  return (
    <form className="stacked-form" onSubmit={handleCreateSubmit} noValidate>
      {errorMessage && (
        <p className="form-message error" role="alert">
          {errorMessage}
        </p>
      )}

      <label>
        User
        <input
          type="text"
          value={term}
          onChange={handleTermChange}
          placeholder="Search by name, username or email"
          autoComplete="off"
          spellCheck="false"
          required
        />
      </label>

      {showResults && (
        <div className="user-search-results">
          {searching && <p className="user-search-status">Searching…</p>}

          {!searching && searchError && (
            <p className="user-search-status error">{searchError}</p>
          )}

          {!searching && !searchError && results.length === 0 && (
            <p className="user-search-status">No users found</p>
          )}

          {!searching && !searchError && results.length > 0 && (
            <ul className="user-search-list">
              {results.map((user) => (
                <li key={user.userId}>
                  <button
                    type="button"
                    className="user-search-option"
                    onClick={() => handleSelect(user)}
                  >
                    <span className="user-search-name">{user.name || user.username}</span>
                    <span className="user-search-email">{user.email}</span>
                  </button>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}

      {selected && (
        <p className="user-search-selected">
          Selected: <strong>{selected.name || selected.username}</strong>
          {selected.email ? ` — ${selected.email}` : ''}
        </p>
      )}

      <div className="modal-actions">
        <button
          type="button"
          className="secondary-button"
          onClick={onCancel}
          disabled={submitting}
        >
          Cancel
        </button>
        <button type="submit" className="primary-button" disabled={submitting || !selected}>
          {submitting ? 'Adding…' : 'Add Member'}
        </button>
      </div>
    </form>
  );
}
