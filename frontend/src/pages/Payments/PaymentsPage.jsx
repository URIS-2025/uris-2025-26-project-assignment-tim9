import { useCallback, useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../auth/useAuth';
import { ApiError } from '../../api/httpClient';
import { getInvoices, getPayments, createInvoice } from '../../api/paymentApi';
import { getProjects } from '../../api/projectApi';
import { InvoiceStatusBadge, PaymentStatusBadge } from '../../components/PaymentStatusBadge';
import { INVOICE_STATUS_ORDER } from '../../utils/paymentStatus';
import { formatMoney, formatDate } from '../../utils/money';
import NewInvoiceForm from './NewInvoiceForm';
import './PaymentsPage.css';

const TABS = [
  { key: 'invoices', label: 'Invoices' },
  { key: 'payments', label: 'Payments' },
];

export default function PaymentsPage() {
  const { token, role } = useAuth();

  const [tab, setTab] = useState('invoices');
  const [invoices, setInvoices] = useState([]);
  const [payments, setPayments] = useState([]);
  const [projects, setProjects] = useState([]);
  const [projectFilter, setProjectFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [newInvoiceOpen, setNewInvoiceOpen] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  //nazivi projekata zive u Project servisu, fakture nose samo projectId
  const projectNames = useMemo(() => {
    const map = new Map();
    projects.forEach((p) => map.set(p.projectId, p.name));
    return map;
  }, [projects]);

  //projekti se ucitavaju jednom - sluze samo za nazive i filter,
  //nema razloga da se povlace na svaku promenu filtera
  useEffect(() => {
    let cancelled = false;
    getProjects(token)
      .then((list) => {
        if (!cancelled) setProjects(list);
      })
      .catch(() => {
        if (!cancelled) setProjects([]);
      });
    return () => {
      cancelled = true;
    };
  }, [token]);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const [invoiceList, paymentList] = await Promise.all([
        getInvoices({ projectId: projectFilter || undefined, status: statusFilter || undefined }, token),
        getPayments({}, token),
      ]);

      setInvoices(invoiceList);
      setPayments(paymentList);
    } catch (err) {
      if (err instanceof ApiError && err.status === 403) {
        setError('You do not have permission to view billing data.');
      } else if (err instanceof ApiError && err.status >= 500) {
        setError('The billing service did not respond. Check that the containers are running.');
      } else {
        setError(err.message || 'Could not load billing data.');
      }
    } finally {
      setLoading(false);
    }
  }, [token, projectFilter, statusFilter]);

  useEffect(() => {
    load();
  }, [load]);

  const rows = tab === 'invoices' ? invoices : payments;
  const canIssue = role === 'Admin' || role === 'ProjectManager';

  return (
    <section className="payments-page">
      <header className="payments-header">
        <div className="payments-title-row">
          <h1 className="payments-title">Billing</h1>
          {!loading && <span className="payments-count">{rows.length}</span>}
          {canIssue && (
            <button
              type="button"
              className="primary-button payments-new"
              onClick={() => setNewInvoiceOpen(true)}
              disabled={projects.length === 0}
            >
              New invoice
            </button>
          )}
        </div>
        <p className="payments-subtitle">
          Invoices issued per project, and payments recorded against them.
        </p>
      </header>

      <div className="payments-tabs" role="tablist">
        {TABS.map((t) => (
          <button
            key={t.key}
            type="button"
            role="tab"
            aria-selected={tab === t.key}
            className={tab === t.key ? 'payments-tab payments-tab-active' : 'payments-tab'}
            onClick={() => setTab(t.key)}
          >
            {t.label}
          </button>
        ))}
      </div>

      {tab === 'invoices' && (
        <div className="payments-filters">
          <label className="payments-filter">
            <span>Project</span>
            <select value={projectFilter} onChange={(e) => setProjectFilter(e.target.value)}>
              <option value="">All projects</option>
              {projects.map((p) => (
                <option key={p.projectId} value={p.projectId}>
                  {p.name}
                </option>
              ))}
            </select>
          </label>

          <label className="payments-filter">
            <span>Status</span>
            <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
              <option value="">All statuses</option>
              {INVOICE_STATUS_ORDER.map((s) => (
                <option key={s} value={s}>
                  {s}
                </option>
              ))}
            </select>
          </label>
        </div>
      )}

      {loading && <p className="status-hint">Loading…</p>}
      {error && !loading && <div className="form-message error">{error}</div>}

      {!loading && !error && rows.length === 0 && (
        <div className="payments-empty">
          <p>{tab === 'invoices' ? 'No invoices yet.' : 'No payments recorded yet.'}</p>
        </div>
      )}

      {!loading && !error && rows.length > 0 && tab === 'invoices' && (
        <table className="payments-table">
          <thead>
            <tr>
              <th>Issued</th>
              <th>Project</th>
              <th className="col-num">Items</th>
              <th className="col-num">Total</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {invoices.map((invoice) => (
              <tr key={invoice.invoiceId}>
                <td>
                  <Link className="payments-link" to={`/payments/${invoice.invoiceId}`}>
                    {formatDate(invoice.issueDate)}
                  </Link>
                </td>
                <td>{projectNames.get(invoice.projectId) || 'Unknown project'}</td>
                <td className="col-num">{invoice.items?.length ?? 0}</td>
                <td className="col-num col-money">{formatMoney(invoice.totalAmount)}</td>
                <td>
                  <InvoiceStatusBadge status={invoice.status} />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {!loading && !error && rows.length > 0 && tab === 'payments' && (
        <table className="payments-table">
          <thead>
            <tr>
              <th>Date</th>
              <th>Invoice</th>
              <th className="col-num">Amount</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {payments.map((payment) => (
              <tr key={payment.paymentId}>
                <td>{formatDate(payment.date)}</td>
                <td>
                  <Link className="payments-link" to={`/payments/${payment.invoiceId}`}>
                    View invoice
                  </Link>
                </td>
                <td className="col-num col-money">{formatMoney(payment.amount)}</td>
                <td>
                  <PaymentStatusBadge status={payment.status} />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {newInvoiceOpen && (
        <NewInvoiceForm
          projects={projects}
          onSubmit={async (values) => {
            await createInvoice(values, token);
            await load();
          }}
          onClose={() => setNewInvoiceOpen(false)}
        />
      )}

      {role === 'Client' && tab === 'payments' && !loading && (
        <p className="payments-note">
          You see every payment you are allowed to read. Invoices you can pay are listed under the
          Invoices tab.
        </p>
      )}
    </section>
  );
}
