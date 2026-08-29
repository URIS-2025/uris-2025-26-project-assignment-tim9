import { useState } from 'react';
import Modal from '../../components/Modal';
import { formatMoney } from '../../utils/money';

const emptyItem = () => ({ description: '', unitPrice: '', quantity: 1 });

// Faktura se izdaje zajedno sa stavkama - servis odbija fakturu bez ijedne stavke.
// Ukupan iznos se ne salje, racuna ga servis; ovde se samo prikazuje.
export default function NewInvoiceForm({ projects, onSubmit, onClose }) {
  const [projectId, setProjectId] = useState(projects[0]?.projectId ?? '');
  const [issueDate, setIssueDate] = useState(() => new Date().toISOString().slice(0, 10));
  const [items, setItems] = useState([emptyItem()]);
  const [error, setError] = useState(null);
  const [busy, setBusy] = useState(false);

  const total = items.reduce(
    (sum, item) => sum + Number(item.unitPrice || 0) * Number(item.quantity || 0),
    0
  );

  function updateItem(index, field, value) {
    setItems((current) =>
      current.map((item, i) => (i === index ? { ...item, [field]: value } : item))
    );
  }

  function removeItem(index) {
    setItems((current) => current.filter((_, i) => i !== index));
  }

  async function handleSubmit(event) {
    event.preventDefault();
    setError(null);

    if (items.length === 0) {
      setError('An invoice needs at least one item.');
      return;
    }

    setBusy(true);
    try {
      await onSubmit({
        projectId,
        //servis odbija datum u buducnosti, pa se salje pocetak dana
        issueDate: `${issueDate}T00:00:00`,
        items: items.map((item) => ({
          description: item.description.trim(),
          unitPrice: Number(item.unitPrice),
          quantity: Number(item.quantity),
        })),
      });
      onClose();
    } catch (err) {
      setError(err.message || 'Could not issue the invoice.');
    } finally {
      setBusy(false);
    }
  }

  return (
    <Modal title="New invoice" onClose={onClose} className="invoice-modal">
      <form className="stacked-form" onSubmit={handleSubmit} noValidate>
        {error && <p className="form-message error">{error}</p>}

        <label>
          Project
          <select value={projectId} onChange={(e) => setProjectId(e.target.value)} required>
            {projects.map((p) => (
              <option key={p.projectId} value={p.projectId}>
                {p.name}
              </option>
            ))}
          </select>
          <span className="field-hint">You must be a member of the project you invoice.</span>
        </label>

        <label>
          Issue date
          <input
            type="date"
            value={issueDate}
            max={new Date().toISOString().slice(0, 10)}
            onChange={(e) => setIssueDate(e.target.value)}
            required
          />
        </label>

        <div className="invoice-items-editor">
          <div className="invoice-items-head">
            <span>Items</span>
            <button
              type="button"
              className="row-action"
              onClick={() => setItems((c) => [...c, emptyItem()])}
            >
              Add row
            </button>
          </div>

          {items.map((item, index) => (
            <div className="invoice-item-row" key={index}>
              <input
                type="text"
                placeholder="Description"
                value={item.description}
                onChange={(e) => updateItem(index, 'description', e.target.value)}
                minLength={2}
                maxLength={200}
                required
              />
              <input
                type="number"
                placeholder="Price"
                step="0.01"
                min="0.01"
                value={item.unitPrice}
                onChange={(e) => updateItem(index, 'unitPrice', e.target.value)}
                required
              />
              <input
                type="number"
                placeholder="Qty"
                step="1"
                min="1"
                value={item.quantity}
                onChange={(e) => updateItem(index, 'quantity', e.target.value)}
                required
              />
              <button
                type="button"
                className="row-action row-action--danger"
                onClick={() => removeItem(index)}
                disabled={items.length === 1}
                aria-label="Remove item"
              >
                ×
              </button>
            </div>
          ))}
        </div>

        <p className="item-preview">
          Invoice total: <strong>{formatMoney(total)}</strong>
          <span> — recalculated by the service on save.</span>
        </p>

        <div className="modal-actions">
          <button type="button" className="secondary-button" onClick={onClose}>
            Cancel
          </button>
          <button type="submit" className="primary-button" disabled={busy}>
            {busy ? 'Issuing…' : 'Issue invoice'}
          </button>
        </div>
      </form>
    </Modal>
  );
}
