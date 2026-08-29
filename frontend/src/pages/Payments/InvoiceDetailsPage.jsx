import { useCallback, useEffect, useMemo, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '../../auth/useAuth';
import { ApiError } from '../../api/httpClient';
import {
  getInvoiceById,
  getInvoiceItems,
  getPayments,
  addInvoiceItem,
  updateInvoiceItem,
  deleteInvoiceItem,
  createPayment,
  deleteInvoice,
} from '../../api/paymentApi';
import { getProjectById } from '../../api/projectApi';
import { InvoiceStatusBadge, PaymentStatusBadge } from '../../components/PaymentStatusBadge';
import InvoiceItemForm from './InvoiceItemForm';
import PayInvoiceForm from './PayInvoiceForm';
import { formatMoney, formatDate } from '../../utils/money';
import './PaymentsPage.css';

const MANAGE_ROLES = ['Admin', 'ProjectManager'];
const PAY_ROLES = ['Admin', 'ProjectManager', 'Client'];

export default function InvoiceDetailsPage() {
  const { invoiceId } = useParams();
  const { token, role } = useAuth();
  const navigate = useNavigate();

  const [invoice, setInvoice] = useState(null);
  const [items, setItems] = useState([]);
  const [payments, setPayments] = useState([]);
  const [project, setProject] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [actionError, setActionError] = useState(null);

  const [itemFormOpen, setItemFormOpen] = useState(false);
  const [editingItem, setEditingItem] = useState(null);
  const [payFormOpen, setPayFormOpen] = useState(false);

  const isPaid = invoice?.status === 'Paid';
  const isCancelled = invoice?.status === 'Cancelled';
  //placena i stornirana faktura su zakljucane za izmene - isto pravilo kao u servisu
  const canManage = MANAGE_ROLES.includes(role) && !isPaid && !isCancelled;
  const canPay = PAY_ROLES.includes(role) && !isPaid && !isCancelled;

  const paidSoFar = useMemo(
    () => payments.filter((p) => p.status === 'Completed').reduce((sum, p) => sum + Number(p.amount), 0),
    [payments]
  );
  const remaining = Math.max(Number(invoice?.totalAmount ?? 0) - paidSoFar, 0);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const [invoiceData, itemList, paymentList] = await Promise.all([
        getInvoiceById(invoiceId, token),
        getInvoiceItems(invoiceId, token),
        getPayments({ invoiceId }, token),
      ]);

      setInvoice(invoiceData);
      setItems(itemList);
      setPayments(paymentList);

      if (invoiceData?.projectId) {
        //naziv projekta zivi u Project servisu; ako je nedostupan, prikaz i dalje radi
        getProjectById(invoiceData.projectId, token)
          .then(setProject)
          .catch(() => setProject(null));
      }
    } catch (err) {
      if (err instanceof ApiError && err.status === 404) {
        setError('That invoice does not exist.');
      } else if (err instanceof ApiError && err.status === 403) {
        setError('You do not have permission to view this invoice.');
      } else {
        setError(err.message || 'Could not load the invoice.');
      }
    } finally {
      setLoading(false);
    }
  }, [invoiceId, token]);

  useEffect(() => {
    load();
  }, [load]);

  async function handleAddItem(values) {
    await addInvoiceItem(invoiceId, values, token);
    await load();
  }

  async function handleUpdateItem(values) {
    await updateInvoiceItem(editingItem.invoiceItemId, values, token);
    await load();
  }

  async function handleDeleteItem(item) {
    setActionError(null);
    try {
      await deleteInvoiceItem(item.invoiceItemId, token);
      await load();
    } catch (err) {
      setActionError(err.message || 'Could not delete the item.');
    }
  }

  async function handlePay(amount) {
    await createPayment({ invoiceId, amount }, token);
    await load();
  }

  async function handleDeleteInvoice() {
    setActionError(null);
    try {
      await deleteInvoice(invoiceId, token);
      navigate('/payments');
    } catch (err) {
      setActionError(err.message || 'Could not delete the invoice.');
    }
  }

  if (loading) return <section className="payments-page"><p className="status-hint">Loading…</p></section>;

  if (error) {
    return (
      <section className="payments-page">
        <div className="form-message error">{error}</div>
        <p className="payments-note">
          <Link className="payments-link" to="/payments">Back to billing</Link>
        </p>
      </section>
    );
  }

  return (
    <section className="payments-page">
      <p className="payments-back">
        <Link className="payments-link" to="/payments">← Billing</Link>
      </p>

      <header className="payments-header">
        <div className="payments-title-row">
          <h1 className="payments-title">Invoice</h1>
          <InvoiceStatusBadge status={invoice.status} />
        </div>
        <p className="payments-subtitle">
          {project?.name || 'Unknown project'} · issued {formatDate(invoice.issueDate)}
        </p>
      </header>

      {actionError && <div className="form-message error">{actionError}</div>}

      <div className="invoice-summary">
        <div className="invoice-stat">
          <span className="invoice-stat-label">Total</span>
          <span className="invoice-stat-value">{formatMoney(invoice.totalAmount)}</span>
        </div>
        <div className="invoice-stat">
          <span className="invoice-stat-label">Paid</span>
          <span className="invoice-stat-value">{formatMoney(paidSoFar)}</span>
        </div>
        <div className="invoice-stat">
          <span className="invoice-stat-label">Remaining</span>
          <span className="invoice-stat-value">{formatMoney(remaining)}</span>
        </div>
      </div>

      <div className="invoice-actions">
        {canPay && (
          <button type="button" className="primary-button" onClick={() => setPayFormOpen(true)}>
            Record payment
          </button>
        )}
        {canManage && (
          <button
            type="button"
            className="secondary-button"
            onClick={() => {
              setEditingItem(null);
              setItemFormOpen(true);
            }}
          >
            Add item
          </button>
        )}
        {MANAGE_ROLES.includes(role) && !isPaid && (
          <button type="button" className="secondary-button" onClick={handleDeleteInvoice}>
            Delete invoice
          </button>
        )}
      </div>

      {isPaid && (
        <p className="payments-note">
          This invoice is paid, so its items can no longer be changed.
        </p>
      )}

      <h2 className="invoice-section-title">Items</h2>
      {items.length === 0 ? (
        <div className="payments-empty"><p>No items on this invoice.</p></div>
      ) : (
        <table className="payments-table">
          <thead>
            <tr>
              <th>Description</th>
              <th className="col-num">Unit price</th>
              <th className="col-num">Qty</th>
              <th className="col-num">Total</th>
              {canManage && <th />}
            </tr>
          </thead>
          <tbody>
            {items.map((item) => (
              <tr key={item.invoiceItemId}>
                <td>{item.description}</td>
                <td className="col-num col-money">{formatMoney(item.unitPrice)}</td>
                <td className="col-num">{item.quantity}</td>
                <td className="col-num col-money">{formatMoney(item.totalAmount)}</td>
                {canManage && (
                  <td className="col-num">
                    <div className="row-actions">
                      <button
                        type="button"
                        className="row-action"
                        onClick={() => {
                          setEditingItem(item);
                          setItemFormOpen(true);
                        }}
                      >
                        Edit
                      </button>
                      <button
                        type="button"
                        className="row-action row-action--danger"
                        onClick={() => handleDeleteItem(item)}
                      >
                        Delete
                      </button>
                    </div>
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>
      )}

      <h2 className="invoice-section-title">Payments</h2>
      {payments.length === 0 ? (
        <div className="payments-empty"><p>No payments recorded yet.</p></div>
      ) : (
        <table className="payments-table">
          <thead>
            <tr>
              <th>Date</th>
              <th className="col-num">Amount</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {payments.map((payment) => (
              <tr key={payment.paymentId}>
                <td>{formatDate(payment.date)}</td>
                <td className="col-num col-money">{formatMoney(payment.amount)}</td>
                <td><PaymentStatusBadge status={payment.status} /></td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {itemFormOpen && (
        <InvoiceItemForm
          item={editingItem}
          onSubmit={editingItem ? handleUpdateItem : handleAddItem}
          onClose={() => {
            setItemFormOpen(false);
            setEditingItem(null);
          }}
        />
      )}

      {payFormOpen && (
        <PayInvoiceForm
          remaining={remaining}
          onSubmit={handlePay}
          onClose={() => setPayFormOpen(false)}
        />
      )}
    </section>
  );
}
