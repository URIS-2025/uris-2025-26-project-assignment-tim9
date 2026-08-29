import { useState } from 'react';
import Modal from '../../components/Modal';

// Jedna forma i za dodavanje i za izmenu stavke. Iznos stavke se ne unosi -
// servis ga racuna kao cena * kolicina, pa se ovde samo prikazuje racunica.
export default function InvoiceItemForm({ item, onSubmit, onClose }) {
  const isEditing = Boolean(item);

  const [description, setDescription] = useState(item?.description ?? '');
  const [unitPrice, setUnitPrice] = useState(item?.unitPrice ?? '');
  const [quantity, setQuantity] = useState(item?.quantity ?? 1);
  const [error, setError] = useState(null);
  const [busy, setBusy] = useState(false);

  const preview = Number(unitPrice || 0) * Number(quantity || 0);

  async function handleSubmit(event) {
    event.preventDefault();
    setError(null);
    setBusy(true);

    try {
      await onSubmit({
        description: description.trim(),
        unitPrice: Number(unitPrice),
        quantity: Number(quantity),
      });
      onClose();
    } catch (err) {
      setError(err.message || 'Could not save the item.');
    } finally {
      setBusy(false);
    }
  }

  return (
    <Modal title={isEditing ? 'Edit item' : 'Add item'} onClose={onClose}>
      <form className="stacked-form" onSubmit={handleSubmit} noValidate>
        {error && <p className="form-message error">{error}</p>}

        <label>
          Description
          <input
            type="text"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            minLength={2}
            maxLength={200}
            required
          />
          <span className="field-hint">Between 2 and 200 characters.</span>
        </label>

        <label>
          Unit price
          <input
            type="number"
            step="0.01"
            min="0.01"
            value={unitPrice}
            onChange={(e) => setUnitPrice(e.target.value)}
            required
          />
        </label>

        <label>
          Quantity
          <input
            type="number"
            step="1"
            min="1"
            value={quantity}
            onChange={(e) => setQuantity(e.target.value)}
            required
          />
        </label>

        <p className="item-preview">
          Line total: <strong>{preview.toFixed(2)}</strong>
          <span> — calculated by the service, not sent from here.</span>
        </p>

        <div className="modal-actions">
          <button type="button" className="secondary-button" onClick={onClose}>
            Cancel
          </button>
          <button type="submit" className="primary-button" disabled={busy}>
            {busy ? 'Saving…' : isEditing ? 'Save changes' : 'Add item'}
          </button>
        </div>
      </form>
    </Modal>
  );
}
