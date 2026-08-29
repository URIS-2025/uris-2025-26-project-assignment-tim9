import { useState } from 'react';

const MOCK_BACKLOG_ITEMS = [
  { id: 'b1', name: 'Podrška za bulk import taskova', description: 'Import iz CSV fajla', createdAt: '2026-08-10T09:00:00' },
  { id: 'b2', name: 'Dark mode', description: 'Odložen za posle roka', createdAt: '2026-08-15T14:30:00' },
  { id: 'b3', name: 'Export projekta u PDF', description: 'Za klijentski izveštaj', createdAt: '2026-08-18T16:45:00' },
];

export default function BacklogView({ projectId }) {
  const [items, setItems] = useState(MOCK_BACKLOG_ITEMS);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');

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

  return (
    <div style={{ maxWidth: '600px', margin: '0 auto', textAlign: 'left' }}>
      <h2 style={{ textAlign: 'center' }}>Backlog</h2>

      <form onSubmit={handleAdd} style={{ marginBottom: '16px' }}>
        <input
          type="text"
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="Naziv stavke"
          style={{
            width: '100%',
            padding: '8px',
            marginBottom: '6px',
            borderRadius: '6px',
            border: '1px solid var(--border)',
            fontFamily: 'var(--sans)',
            boxSizing: 'border-box',
          }}
        />
        <textarea
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          placeholder="Opis"
          style={{
            width: '100%',
            minHeight: '50px',
            marginBottom: '6px',
            padding: '8px',
            borderRadius: '6px',
            border: '1px solid var(--border)',
            fontFamily: 'var(--sans)',
            boxSizing: 'border-box',
          }}
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
          Dodaj u backlog
        </button>
      </form>

      {items.map((item) => (
        <div
          key={item.id}
          style={{
            border: '1px solid var(--border)',
            borderRadius: '6px',
            padding: '10px 12px',
            marginBottom: '8px',
          }}
        >
          <strong>{item.name}</strong>
          <p style={{ margin: '4px 0', color: 'var(--text)' }}>{item.description}</p>
          <p style={{ margin: 0, fontSize: '12px', color: 'var(--text)' }}>
            Dodato: {new Date(item.createdAt).toLocaleString('sr-RS')}
          </p>
        </div>
      ))}
    </div>
  );
}
