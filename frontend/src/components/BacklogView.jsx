import { useState } from 'react';

const MOCK_BACKLOG_ITEMS = [
  { id: 'b1', name: 'Bulk task import support', description: 'Import from a CSV file', createdAt: '2026-08-10T09:00:00' },
  { id: 'b2', name: 'Dark mode', description: 'Postponed until after the deadline', createdAt: '2026-08-15T14:30:00' },
  { id: 'b3', name: 'Export project to PDF', description: 'For the client report', createdAt: '2026-08-18T16:45:00' },
];

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
  const [items, setItems] = useState(MOCK_BACKLOG_ITEMS);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');

  const [editingId, setEditingId] = useState(null);
  const [editName, setEditName] = useState('');
  const [editDescription, setEditDescription] = useState('');

  function handleAdd(e) {
    e.preventDefault();
    if (!name.trim()) return;

    const newItem = {
      id: crypto.randomUUID(),
      name,
      description,
      createdAt: new Date().toISOString(),
    };

    setItems([newItem, ...items]);
    setName('');
    setDescription('');
  }

  function handleDelete(id) {
    if (!window.confirm('Delete this backlog item?')) return;
    setItems((prev) => prev.filter((item) => item.id !== id));
  }

  function handleEditStart(item) {
    setEditingId(item.id);
    setEditName(item.name);
    setEditDescription(item.description);
  }

  function handleEditCancel() {
    setEditingId(null);
    setEditName('');
    setEditDescription('');
  }

  function handleEditSave() {
    setItems((prev) =>
      prev.map((item) =>
        item.id === editingId ? { ...item, name: editName, description: editDescription } : item,
      ),
    );
    handleEditCancel();
  }

  function handleMoveToSprint(item) {
    const sprintName = window.prompt('Enter Sprint name (mock - real integration coming later):');
    if (!sprintName || !sprintName.trim()) return;

    setItems((prev) => prev.filter((current) => current.id !== item.id));
    window.alert(
      `Item moved to sprint: ${sprintName.trim()}. (Mock action - not yet connected to SprintService.)`,
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
        const isEditing = editingId === item.id;

        return (
          <div
            key={item.id}
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
                onClick={() => handleDelete(item.id)}
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
