import { useState } from 'react';
import Modal from '../../components/Modal';
import { formatMoney } from '../../utils/money';

// Uplata ne sme da premasi preostali dug - servis to odbija sa 409,
// a forma isto racuna preostalo da korisnik ne saznaje tek posle slanja.
export default function PayInvoiceForm({ remaining, onSubmit, onClose }) {
  const [amount, setAmount] = useState(remaining > 0 ? remaining : '');
  const [error, setError] = useState(null);
  const [busy, setBusy] = useState(false);

  const tooMuch = Number(amount) > remaining;

  async function handleSubmit(event) {
    event.preventDefault();
    setError(null);
    setBusy(true);

    try {
      await onSubmit(Number(amount));
      onClose();
    } catch (err) {
      setError(err.message || 'Payment failed.');
    } finally {
      setBusy(false);
    }
  }

  return (
    <Modal title="Record a payment" onClose={onClose}>
      <form className="stacked-form" onSubmit={handleSubmit} noValidate>
        {error && <p className="form-message error">{error}</p>}

        <p className="pay-remaining">
          Remaining on this invoice: <strong>{formatMoney(remaining)}</strong>
        </p>

        <label>
          Amount
          <input
            type="number"
            step="0.01"
            min="0.01"
            max={remaining}
            value={amount}
            onChange={(e) => setAmount(e.target.value)}
            required
          />
          {tooMuch && (
            <span className="field-hint error">
              More than the remaining debt — the service will reject this.
            </span>
          )}
        </label>

        <div className="modal-actions">
          <button type="button" className="secondary-button" onClick={onClose}>
            Cancel
          </button>
          <button type="submit" className="primary-button" disabled={busy || tooMuch}>
            {busy ? 'Recording…' : 'Record payment'}
          </button>
        </div>
      </form>
    </Modal>
  );
}
