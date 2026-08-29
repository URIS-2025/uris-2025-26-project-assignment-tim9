// Isti obrazac kao utils/projectStatus.js - status se prevodi u "ton",
// a tonovi su vec definisani kao .status-pill--* klase.

export const INVOICE_STATUS_ORDER = ['Unpaid', 'Paid', 'Cancelled'];

export const INVOICE_STATUS_META = {
  Unpaid: { label: 'Unpaid', tone: 'in-progress' },
  Paid: { label: 'Paid', tone: 'done' },
  Cancelled: { label: 'Cancelled', tone: 'critical' },
};

export const PAYMENT_STATUS_ORDER = ['Pending', 'Completed', 'Failed', 'Refunded'];

export const PAYMENT_STATUS_META = {
  Pending: { label: 'Pending', tone: 'in-progress' },
  Completed: { label: 'Completed', tone: 'done' },
  Failed: { label: 'Failed', tone: 'critical' },
  Refunded: { label: 'Refunded', tone: 'neutral' },
};

function resolve(order, meta, status) {
  //servis salje enum kao tekst, ali brojcani oblik se isto pokriva
  const key = typeof status === 'number' ? order[status] : status;
  const found = meta[key];
  if (found) return { key, ...found };
  return { key, label: status == null ? 'Unknown' : String(status), tone: 'neutral' };
}

export function resolveInvoiceStatus(status) {
  return resolve(INVOICE_STATUS_ORDER, INVOICE_STATUS_META, status);
}

export function resolvePaymentStatus(status) {
  return resolve(PAYMENT_STATUS_ORDER, PAYMENT_STATUS_META, status);
}
