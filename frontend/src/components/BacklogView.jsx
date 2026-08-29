import { useEffect, useState } from 'react';
import { useAuth } from '../auth/useAuth';
import {
  getBacklog,
  addBacklogItem,
  deleteBacklogItem,
  updateBacklogItem,
} from '../api/workPackageApi';

const inputStyle = {
  width: '100%',
  padding: '8px',
  marginBottom: '6px',
  borderRadius: '6px',
  border: '1px solid var(--border)',
  fontFamily: 'var(--sans)',
  boxSizing: 'border-box',
};

const secondaryButtonStyle = {
  padding: '4px 10px',
  fontSize: '12px',
  fontFamily: 'var(--sans)',
  borderRadius: '6px',
  border: '1px solid var(--border)',
  background: 'transparent',
  color: 'var(--text)',
  cursor: 'pointer',
};

const primaryButtonStyle = {
  ...secondaryButtonStyle,
  border: '1px solid var(--accent)',
  background: 'var(--accent)',
  color: '#fff',
};

export default function BacklogView({ projectId }) {
  const { token, userId } = useAuth();
  const [items, setItems] = useState([]);
  const [phase, setPhase] = useState('loading');
  const [errorMessage, setErrorMessage] = useState('');
  const [reloadKey, setReloadKey] = useState(0);

  const [name, setName] = useState('');
  const [description, setDescription] = useState('');

  const [editingId, setEditingId] = useState(null);
  const [editName, setEditName] = useState('');
  const [editDescription, setEditDescription] = useState('');

  useEffect(() => {
    let ignore = false;

    getBacklog(projectId, token)
      .then((data) => {
        if (ignore) return;
        setItems(Array.isArray(data) ? data : []);
        setErrorMessage('');
        setPhase('ready');
      })
      .catch((error) => {
        if (ignore) return;
        setErrorMessage(
          error && error.status === 401
            ? 'Your session has expired. Please sign in again.'
            : 'Something went wrong while loading the backlog.',
        );
        setPhase('error');
      });

    return () => {
      ignore = true;
    };
  }, [projectId, token, reloadKey]);

  const reload = () => {
    setPhase('loading');
    setReloadKey((key) => key + 1);
  };

  async function handleAdd(e) {
    e.preventDefault();
    if (!name.trim()) return;
    try {
      await addBacklogItem(projectId, { name: name.trim(), description }, token, userId);
      setName('');
      setDescription('');
      reload();
    } catch (error) {
      window.alert(error?.message || 'Could not add the backlog item.');
    }
  }

  async function handleDelete(id) {
    if (!window.confirm('Delete this backlog item?')) return;
    try {
      await deleteBacklogItem(id, token);
      setItems((prev) => prev.filter((item) => item.backlogId !== id));
    } catch (error) {
      window.alert(error?.message || 'Could not delete the backlog item.');
    }
  }

  function handleEditStart(item) {
    setEditingId(item.backlogId);
    setEditName(item.name);
    setEditDescription(item.description ?? '');
  }

  function handleEditCancel() {
    setEditingId(null);
    setEditName('');
    setEditDescription('');
  }

  async function handleEditSave() {
    try {
      await updateBacklogItem(editingId, { name: editName, description: editDescription }, token);
      setItems((prev) =>
        prev.map((item) =>
          item.backlogId === editingId ? { ...item, name: editName, description: editDescription } : item,
        ),
      );
      handleEditCancel();
    } catch (error) {
      window.alert(error?.message || 'Could not save the backlog item.');
    }
  }

  function handleMoveToSprint(item) {
    const sprintName = window.prompt('Enter Sprint name (mock - real integration coming later):');
    if (!sprintName || !sprintName.trim()) return;

    setItems((prev) => prev.filter((current) => current.backlogId !== item.backlogId));
    window.alert(
      `Item moved to sprint: ${sprintName.trim()}. (Mock action - not yet connected to SprintService.)`,
    );
  }

  if (phase === 'loading') {
    return <p style={{ maxWidth: '600px', margin: '0 auto' }}>Loading...</p>;
  }
  if (phase === 'error') {
    return (
      <p style={{ maxWidth: '600px', margin: '0 auto' }}>
        {errorMessage}{' '}
        <button type="button" onClick={reload}>
          Retry
        </button>
      </p>
    );
  }

  return (
    <div style={{ maxWidth: '600px', margin: '0 auto', textAlign: 'left' }}>
      <form onSubmit={handleAdd} style={{ marginBottom: '16px' }}>
        <input
          type="text"
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="Item name"
          style={inputStyle}
        />
        <textarea
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          placeholder="Description"
          style={{ ...inputStyle, minHeight: '50px' }}
        />
        <button
          type="submit"
          style={{
            background: 'var(--accent)',
            color: '#fff',
            border: 'none',
            borderRadius: '6px',
            padding: '8px 16px',
            cursor: 'pointer',
            fontFamily: 'var(--sans)',
          }}
        >
          Add to backlog
        </button>
      </form>

      {items.map((item) => {
        const isEditing = editingId === item.backlogId;

        return (
          <div
            key={item.backlogId}
            style={{
              position: 'relative',
              border: '1px solid var(--border)',
              borderRadius: '6px',
              padding: '10px 12px',
              marginBottom: '8px',
            }}
          >
            {!isEditing && (
              <button
                type="button"
                aria-label="Delete backlog item"
                onClick={() => handleDelete(item.backlogId)}
                style={{
                  position: 'absolute',
                  top: '8px',
                  right: '8px',
                  width: '24px',
                  height: '24px',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  border: 'none',
                  borderRadius: '6px',
                  background: 'transparent',
                  color: 'var(--color-status-critical)',
                  fontSize: '18px',
                  lineHeight: 1,
                  cursor: 'pointer',
                }}
              >
                ×
              </button>
            )}

            {isEditing ? (
              <>
                <input
                  type="text"
                  value={editName}
                  onChange={(e) => setEditName(e.target.value)}
                  placeholder="Item name"
                  style={inputStyle}
                />
                <textarea
                  value={editDescription}
                  onChange={(e) => setEditDescription(e.target.value)}
                  placeholder="Description"
                  style={{ ...inputStyle, minHeight: '50px' }}
                />
                <div style={{ display: 'flex', gap: '8px', marginTop: '2px' }}>
                  <button type="button" onClick={handleEditSave} style={primaryButtonStyle}>
                    Save
                  </button>
                  <button type="button" onClick={handleEditCancel} style={secondaryButtonStyle}>
                    Cancel
                  </button>
                </div>
              </>
            ) : (
              <>
                <strong style={{ display: 'block', paddingRight: '24px' }}>{item.name}</strong>
                <p style={{ margin: '4px 0', color: 'var(--text)' }}>{item.description}</p>
                <p style={{ margin: '0 0 8px', fontSize: '12px', color: 'var(--text)' }}>
                  Added: {new Date(item.createdAt).toLocaleString('en-GB')}
                </p>
                <div style={{ display: 'flex', gap: '8px', flexWrap: 'wrap' }}>
                  <button type="button" onClick={() => handleEditStart(item)} style={secondaryButtonStyle}>
                    ✎ Edit
                  </button>
                  <button type="button" onClick={() => handleMoveToSprint(item)} style={secondaryButtonStyle}>
                    Move to Sprint
                  </button>
                </div>
              </>
            )}
          </div>
        );
      })}
    </div>
  );
}
