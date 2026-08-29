import { apiRequest } from './httpClient';

// Sve ide kroz API Gateway na PaymentService.
// Identitet korisnika servis cita iz tokena, pa se X-User-Id ne salje.

/* ---------------- fakture ---------------- */

// GET /api/invoice?projectId=&status=
export function getInvoices({ projectId, status } = {}, token) {
  return apiRequest('/api/invoice', { token, query: { projectId, status } }).then((r) => r || []);
}

// GET /api/invoice/{invoiceId}
export function getInvoiceById(invoiceId, token) {
  return apiRequest(`/api/invoice/${invoiceId}`, { token });
}

// POST /api/invoice - Admin i ProjectManager, uz clanstvo na projektu
export function createInvoice({ projectId, issueDate, items }, token) {
  return apiRequest('/api/invoice', {
    method: 'POST',
    token,
    body: { projectId, issueDate, items },
  });
}

// PUT /api/invoice/{invoiceId} - placena faktura vraca 409
export function updateInvoice(invoiceId, changes, token) {
  return apiRequest(`/api/invoice/${invoiceId}`, { method: 'PUT', token, body: changes });
}

// DELETE /api/invoice/{invoiceId} - brise i stavke i uplate
export function deleteInvoice(invoiceId, token) {
  return apiRequest(`/api/invoice/${invoiceId}`, { method: 'DELETE', token });
}

/* ---------------- stavke fakture ---------------- */

// GET /api/invoice/{invoiceId}/items
export function getInvoiceItems(invoiceId, token) {
  return apiRequest(`/api/invoice/${invoiceId}/items`, { token }).then((r) => r || []);
}

// POST /api/invoice/{invoiceId}/items - iznos stavke racuna servis
export function addInvoiceItem(invoiceId, { description, unitPrice, quantity }, token) {
  return apiRequest(`/api/invoice/${invoiceId}/items`, {
    method: 'POST',
    token,
    body: { description, unitPrice, quantity },
  });
}

// PUT /api/invoiceitem/{invoiceItemId}
export function updateInvoiceItem(invoiceItemId, changes, token) {
  return apiRequest(`/api/invoiceitem/${invoiceItemId}`, { method: 'PUT', token, body: changes });
}

// DELETE /api/invoiceitem/{invoiceItemId}
export function deleteInvoiceItem(invoiceItemId, token) {
  return apiRequest(`/api/invoiceitem/${invoiceItemId}`, { method: 'DELETE', token });
}

/* ---------------- uplate ---------------- */

// GET /api/payment?invoiceId=&paidByUserId=
export function getPayments({ invoiceId, paidByUserId } = {}, token) {
  return apiRequest('/api/payment', { token, query: { invoiceId, paidByUserId } }).then(
    (r) => r || []
  );
}

// GET /api/payment/{paymentId}
export function getPaymentById(paymentId, token) {
  return apiRequest(`/api/payment/${paymentId}`, { token });
}

// POST /api/payment - Admin, ProjectManager i Client, uz clanstvo na projektu
export function createPayment({ invoiceId, amount }, token) {
  return apiRequest('/api/payment', { method: 'POST', token, body: { invoiceId, amount } });
}

// PUT /api/payment/{paymentId}
export function updatePayment(paymentId, changes, token) {
  return apiRequest(`/api/payment/${paymentId}`, { method: 'PUT', token, body: changes });
}

// DELETE /api/payment/{paymentId} - samo Admin
export function deletePayment(paymentId, token) {
  return apiRequest(`/api/payment/${paymentId}`, { method: 'DELETE', token });
}
