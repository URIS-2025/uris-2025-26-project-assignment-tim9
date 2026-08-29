import { useEffect, useState } from 'react';
import { createProjectMember } from '../api/projectApi';
import { searchUsers } from '../api/userApi';
import '../shared/styles/forms.css';
import './ProjectMemberForm.css';

const MIN_QUERY = 2;

function displayName(user) {
  return user.name || user.username || user.userId;
}

export default function ProjectMemberForm({ projectId, token, onCreated, onCancel }) {
  const [term, setTerm] = useState('');
  const [selected, setSelected] = useState(null);
  const [results, setResults] = useState([]);
  const [searching, setSearching] = useState(false);
  const [searchError, setSearchError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');

  const query = term.trim();
  const showResults = !selected && query.length >= MIN_QUERY;

  useEffect(() => {
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
  }, [query, token, selected]);

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

  const handleSubmit = async (event) => {
    event.preventDefault();
    if (!selected) {
      setErrorMessage('Please pick a user from the search results.');
      return;
    }

    setSubmitting(true);
    setErrorMessage('');
    try {
      await createProjectMember({ projectId, userId: selected.userId }, token);
      onCreated();
    } catch (error) {
      const status = error && error.status;
      let message;
      if (status === 403) {
        message = "You don't have permission to add members.";
      } else if (status === 401) {
        message = 'Your session has expired. Please sign in again.';
      } else if (status === 400) {
        message = error.message || 'The user could not be added. Please try another user.';
      } else {
        message = 'Something went wrong while adding the member. Please try again.';
      }
      setErrorMessage(message);
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
        <button
          type="submit"
          className="primary-button"
          disabled={submitting || !selected}
        >
          {submitting ? 'Adding…' : 'Add Member'}
        </button>
      </div>
    </form>
  );
}
