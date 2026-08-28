import { apiRequest, ApiError } from './httpClient';

// AttachmentService, reached through the gateway's /attachments route (see
// ApiGateway/appsettings.json). Unlike the other API modules, every call
// here needs X-User-Id too - AttachmentController reads the acting user from
// that header (set by whoever calls it - normally the gateway, but nothing
// upstream of AttachmentService does that yet, so this module sends it
// itself, same as httpClient's userId option was already built for).

// GET /attachments?projectId=&taskId= (via gateway) -> AttachmentService
// Exactly one of projectId/taskId should be set - the backend scopes to
// whichever is present (taskId wins if both are).
export function getAttachments({ projectId, taskId }, token, userId) {
  return apiRequest('/attachments', {
    token,
    userId,
    query: { projectId, taskId },
  }).then((r) => r || []);
}

// POST /attachments/upload (via gateway) -> AttachmentService
// Registers the attachment and returns a short-lived presigned PUT URL for
// the actual bytes - the caller uploads directly to storage next (see
// uploadFileToStorage), then confirms with confirmAttachment.
export function createAttachmentUpload({ originalFileName, contentType, fileSize, projectId, taskId }, token, userId) {
  return apiRequest('/attachments/upload', {
    method: 'POST',
    token,
    userId,
    body: { originalFileName, contentType, fileSize, projectId, taskId: taskId || null },
  });
}

// PUT straight to object storage using the presigned URL from
// createAttachmentUpload - this never goes through the gateway.
export async function uploadFileToStorage(uploadUrl, file) {
  let response;
  try {
    response = await fetch(uploadUrl, {
      method: 'PUT',
      headers: { 'Content-Type': file.type || 'application/octet-stream' },
      body: file,
    });
  } catch {
    throw new ApiError(0, 'Could not reach storage to upload the file.');
  }
  if (!response.ok) {
    throw new ApiError(response.status, 'Storage rejected the file upload.');
  }
}

// SHA-256 of the file, hex-encoded, for AttachmentConfirmationDTO.Checksum.
// Optional (the backend only records it if present) - if the browser can't
// compute it (e.g. non-secure context, so no window.crypto.subtle) the
// upload still proceeds without one.
export async function computeFileChecksum(file) {
  if (!window.crypto?.subtle) return null;
  try {
    const buffer = await file.arrayBuffer();
    const digest = await window.crypto.subtle.digest('SHA-256', buffer);
    return Array.from(new Uint8Array(digest))
      .map((b) => b.toString(16).padStart(2, '0'))
      .join('');
  } catch {
    return null;
  }
}

// POST /attachments/confirm (via gateway) -> AttachmentService
// Marks the attachment Ready once the bytes are actually in storage.
export function confirmAttachment({ attachmentId, checksum }, token, userId) {
  return apiRequest('/attachments/confirm', {
    method: 'POST',
    token,
    userId,
    body: { attachmentId, checksum: checksum || null },
  });
}

// PUT /attachments/{id} (via gateway) -> AttachmentService
// Renames and/or redescribes an attachment. Owner or Admin only (enforced
// server-side).
export function updateAttachment(id, { fileName, description }, token, userId) {
  return apiRequest(`/attachments/${id}`, {
    method: 'PUT',
    token,
    userId,
    body: { fileName: fileName ?? null, description: description ?? null },
  });
}

// DELETE /attachments/{id} (via gateway) -> AttachmentService
// Soft-deletes; owner or Admin only (enforced server-side).
export function deleteAttachment(id, token, userId) {
  return apiRequest(`/attachments/${id}`, { method: 'DELETE', token, userId });
}

// GET /attachments/{id}/download (via gateway) -> AttachmentService, which
// 302s to a presigned storage URL. apiRequest can't be reused here since it
// assumes a JSON body; fetch follows the redirect on its own and hands back
// the file bytes as a blob for the caller to save.
export async function downloadAttachmentFile(id, token, userId) {
  let response;
  try {
    response = await fetch(`/attachments/${id}/download`, {
      headers: {
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
        ...(userId ? { 'X-User-Id': userId } : {}),
      },
    });
  } catch {
    throw new ApiError(0, 'Could not reach the API Gateway to download this file.');
  }
  if (!response.ok) {
    throw new ApiError(response.status, `Could not download this file (status ${response.status}).`);
  }
  return response.blob();
}
