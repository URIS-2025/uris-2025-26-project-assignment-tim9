import { resolveInvoiceStatus, resolvePaymentStatus } from '../utils/paymentStatus';

// .status-pill klase su globalne (dolaze iz ProjectListPage.css),
// pa se koriste iste oznake kao na projektima i zadacima.
export function InvoiceStatusBadge({ status }) {
  const { label, tone } = resolveInvoiceStatus(status);
  return <span className={`status-pill status-pill--${tone}`}>{label}</span>;
}

export function PaymentStatusBadge({ status }) {
  const { label, tone } = resolvePaymentStatus(status);
  return <span className={`status-pill status-pill--${tone}`}>{label}</span>;
}
