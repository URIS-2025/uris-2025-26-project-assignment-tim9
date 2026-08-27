import { useEffect, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import Modal from './Modal';
import { useAuth } from '../auth/useAuth';
import { ApiError } from '../api/httpClient';
import {
  getAttachments,
  createAttachmentUpload,
  uploadFileToStorage,
  computeFileChecksum,
  confirmAttachment,
  updateAttachment,
  deleteAttachment,
  downloadAttachmentFile,
} from '../api/attachmentApi';
import './AttachmentsButton.css';

// AttachmentController's upload route is [Authorize] with an
// EnsureAllowedToUploadAsync check that only lets TeamMember/ProjectManager
// (or Admin, checked separately) create attachments - keep this in sync with
// AttachmentRepository.UploaderRoles if the backend's allowed roles change.
const UPLOAD_ROLES = ['Admin', 'ProjectManager', 'TeamMember'];

// AttachmentStatus enum (AttachmentService.Models.Enums.AttachmentStatus) -
// serialized as a plain number, same story as the other services' enums.
const STATUS_READY = 2;

function isPreviewable(a) {
  return typeof a.contentType === 'string' && a.contentType.startsWith('image/');
}

function formatFileSize(bytes) {
  if (!Number.isFinite(bytes)) return '';
  if (bytes < 1024) return `${bytes} B`;
  const units = ['KB', 'MB', 'GB'];
  let value = bytes / 1024;
  let unit = 0;
  while (value >= 1024 && unit < units.length - 1) {
    value /= 1024;
    unit += 1;
  }
  return `${value.toFixed(value < 10 ? 1 : 0)} ${units[unit]}`;
}

function PaperclipIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" aria-hidden="true">
      <path
        d="M17.5 8.5 9.9 16.1a3 3 0 1 1-4.24-4.24l8.13-8.13a2 2 0 1 1 2.83 2.83l-7.78 7.78a1 1 0 1 1-1.41-1.41l7.07-7.07"
        stroke="currentColor"
        strokeWidth="1.6"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );
}

function KebabIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
      <circle cx="12" cy="5" r="1.8" />
      <circle cx="12" cy="12" r="1.8" />
      <circle cx="12" cy="19" r="1.8" />
    </svg>
  );
}

function DownloadIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" aria-hidden="true">
      <path
        d="M12 3v12m0 0-4-4m4 4 4-4M5 19h14"
        stroke="currentColor"
        strokeWidth="1.6"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );
}

/**
 * A button that opens a form for attaching files to a project or task, plus
 * the list of files already attached. Not a page of its own - it's meant to
 * be dropped into whatever screen already has the project/task in context
 * (a task row, a project header, ...).
 *
 * @param {object} props
 * @param {string} props.projectId - required; every attachment belongs to a project
 * @param {string} [props.taskId] - if set, scopes to this task instead of the project directly
 * @param {string} [props.label] - trigger button text (default "Attachments")
 */
export default function AttachmentsButton({ projectId, taskId, label = 'Attachments' }) {
  const { token, userId, role } = useAuth();
  const canUpload = UPLOAD_ROLES.includes(role);

  const [isOpen, setIsOpen] = useState(false);
  const [attachments, setAttachments] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const fileInputRef = useRef(null);
  const [uploading, setUploading] = useState(false);
  const [uploadError, setUploadError] = useState(null);

  // The kebab menu is portal-rendered to document.body (see below) so it can
  // never be clipped by the scrolling attachment list or the modal's own
  // overflow - menuAnchor is the trigger button's viewport position, read
  // fresh each time the menu opens.
  const [openMenuId, setOpenMenuId] = useState(null);
  const [menuAnchor, setMenuAnchor] = useState(null); // { top, right } in viewport px
  const menuRef = useRef(null);

  const [editingId, setEditingId] = useState(null);
  const [editForm, setEditForm] = useState({ fileName: '', description: '' });
  const [editError, setEditError] = useState(null);
  const [savingEdit, setSavingEdit] = useState(false);

  const [downloadingId, setDownloadingId] = useState(null);
  const [removingId, setRemovingId] = useState(null);

  const [preview, setPreview] = useState(null); // { attachment, url } | null
  const [previewLoading, setPreviewLoading] = useState(false);
  const [previewError, setPreviewError] = useState(null);

  function handleAuthError(err) {
    return err instanceof ApiError && err.status === 401;
  }

  useEffect(() => {
    if (!isOpen) return;
    let cancelled = false;
    (async () => {
      setLoading(true);
      setError(null);
      try {
        const list = await getAttachments({ projectId, taskId }, token, userId);
        if (!cancelled) setAttachments(list);
      } catch (err) {
        if (!cancelled) setError(err.message || 'Could not load attachments.');
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [isOpen, projectId, taskId, token, userId]);

  // Close the kebab menu on an outside click, or on any scroll (its position
  // was computed once when it opened, so it'd otherwise drift out from under
  // its trigger button as soon as the list - or the modal - scrolls).
  useEffect(() => {
    if (openMenuId === null) return undefined;
    function handleMousedown(e) {
      if (menuRef.current?.contains(e.target)) return;
      // The trigger button's own onClick already toggles the menu - without
      // this, mousedown-before-click would close it here first, and the
      // click's toggle would then see it as already-closed and reopen it.
      if (e.target.closest?.('.attachment-menu-trigger')) return;
      setOpenMenuId(null);
    }
    function handleScroll() {
      setOpenMenuId(null);
    }
    document.addEventListener('mousedown', handleMousedown);
    document.addEventListener('scroll', handleScroll, true);
    window.addEventListener('resize', handleScroll);
    return () => {
      document.removeEventListener('mousedown', handleMousedown);
      document.removeEventListener('scroll', handleScroll, true);
      window.removeEventListener('resize', handleScroll);
    };
  }, [openMenuId]);

  function closeModal() {
    setIsOpen(false);
    setOpenMenuId(null);
    setEditingId(null);
    setUploadError(null);
    closePreview();
  }

  function closePreview() {
    setPreview((prev) => {
      if (prev?.url) URL.revokeObjectURL(prev.url);
      return null;
    });
    setPreviewError(null);
  }

  async function handlePreview(a) {
    setOpenMenuId(null);
    setPreviewError(null);
    setPreviewLoading(true);
    setPreview({ attachment: a, url: null });
    try {
      const blob = await downloadAttachmentFile(a.id, token, userId);
      const url = URL.createObjectURL(blob);
      setPreview({ attachment: a, url });
    } catch (err) {
      if (handleAuthError(err)) {
        setPreview(null);
      } else {
        setPreviewError(err.message || 'Could not load this preview.');
      }
    } finally {
      setPreviewLoading(false);
    }
  }

  function openFilePicker() {
    fileInputRef.current?.click();
  }

  async function handleFileSelected(e) {
    const file = e.target.files?.[0];
    e.target.value = ''; // so picking the same file again still fires onChange
    if (!file) return;

    setUploadError(null);
    setUploading(true);
    try {
      const created = await createAttachmentUpload(
        {
          originalFileName: file.name,
          contentType: file.type,
          fileSize: file.size,
          projectId,
          taskId,
        },
        token,
        userId
      );
      await uploadFileToStorage(created.uploadUrl, file);
      const checksum = await computeFileChecksum(file);
      const confirmed = await confirmAttachment(
        { attachmentId: created.attachment.id, checksum },
        token,
        userId
      );
      setAttachments((prev) => [confirmed, ...prev]);
    } catch (err) {
      if (!handleAuthError(err)) {
        setUploadError(err.message || 'Could not upload this file.');
      }
    } finally {
      setUploading(false);
    }
  }

  function toggleMenu(e, a) {
    if (openMenuId === a.id) {
      setOpenMenuId(null);
      return;
    }
    const rect = e.currentTarget.getBoundingClientRect();
    setMenuAnchor({ top: rect.bottom + 4, right: window.innerWidth - rect.right });
    setOpenMenuId(a.id);
  }

  function startEdit(a) {
    setOpenMenuId(null);
    setEditingId(a.id);
    setEditForm({ fileName: a.fileName, description: a.description || '' });
    setEditError(null);
  }

  function cancelEdit() {
    setEditingId(null);
    setEditError(null);
  }

  async function handleSaveEdit(e, id) {
    e.preventDefault();
    if (!editForm.fileName.trim()) {
      setEditError('File name cannot be empty.');
      return;
    }
    setEditError(null);
    setSavingEdit(true);
    try {
      const updated = await updateAttachment(
        id,
        { fileName: editForm.fileName.trim(), description: editForm.description.trim() || null },
        token,
        userId
      );
      setAttachments((prev) => prev.map((a) => (a.id === id ? updated : a)));
      setEditingId(null);
    } catch (err) {
      if (!handleAuthError(err)) {
        setEditError(err.message || 'Could not update this attachment.');
      }
    } finally {
      setSavingEdit(false);
    }
  }

  async function handleRemove(a) {
    setOpenMenuId(null);
    if (!window.confirm(`Remove "${a.fileName}"? This cannot be undone.`)) return;
    setRemovingId(a.id);
    setError(null);
    try {
      await deleteAttachment(a.id, token, userId);
      setAttachments((prev) => prev.filter((x) => x.id !== a.id));
    } catch (err) {
      if (!handleAuthError(err)) {
        setError(err.message || 'Could not remove this attachment.');
      }
    } finally {
      setRemovingId(null);
    }
  }

  async function handleDownload(a) {
    setDownloadingId(a.id);
    setError(null);
    try {
      const blob = await downloadAttachmentFile(a.id, token, userId);
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = a.originalFileName || a.fileName;
      document.body.appendChild(link);
      link.click();
      link.remove();
      URL.revokeObjectURL(url);
    } catch (err) {
      if (!handleAuthError(err)) {
        setError(err.message || 'Could not download this file.');
      }
    } finally {
      setDownloadingId(null);
    }
  }

  return (
    <>
      <button
        type="button"
        className="secondary-button attachments-trigger"
        onClick={() => setIsOpen(true)}
      >
        <PaperclipIcon />
        {label}
        {attachments.length > 0 && <span className="attachments-count">{attachments.length}</span>}
      </button>

      {isOpen && (
        <Modal title="Attachments" onClose={closeModal} className="attachments-modal">
          <div className="attachments-panel">
            {canUpload && (
              <div className="attachment-upload">
                <p className="attachment-upload-text">Attach a file from your computer</p>
                <button
                  type="button"
                  className="primary-button"
                  onClick={openFilePicker}
                  disabled={uploading}
                >
                  {uploading ? 'Uploading…' : 'Choose a file'}
                </button>
                <span className="attachment-upload-hint">Up to 25 MB.</span>
                <input
                  ref={fileInputRef}
                  type="file"
                  className="attachment-file-input"
                  onChange={handleFileSelected}
                />
                {uploadError && <div className="form-message error">{uploadError}</div>}
              </div>
            )}

            <div className="attachment-list-section">
              <h3>Files{attachments.length > 0 ? ` (${attachments.length})` : ''}</h3>

              {error && <div className="form-message error">{error}</div>}

              {loading ? (
                <p className="status-hint">Loading attachments…</p>
              ) : attachments.length === 0 ? (
                <p className="attachment-empty">No files attached yet.</p>
              ) : (
                <ul className="attachment-list">
                  {attachments.map((a) => (
                    <li className="attachment-row" key={a.id}>
                      {editingId === a.id ? (
                        <form className="attachment-edit-form" onSubmit={(e) => handleSaveEdit(e, a.id)}>
                          <input
                            className="attachment-edit-name"
                            value={editForm.fileName}
                            onChange={(e) => setEditForm((f) => ({ ...f, fileName: e.target.value }))}
                            autoFocus
                          />
                          <input
                            className="attachment-edit-desc"
                            placeholder="Description (optional)"
                            value={editForm.description}
                            onChange={(e) => setEditForm((f) => ({ ...f, description: e.target.value }))}
                          />
                          {editError && <div className="form-message error">{editError}</div>}
                          <div className="attachment-edit-actions">
                            <button type="button" className="secondary-button" onClick={cancelEdit}>
                              Cancel
                            </button>
                            <button type="submit" className="primary-button" disabled={savingEdit}>
                              {savingEdit ? 'Saving…' : 'Save'}
                            </button>
                          </div>
                        </form>
                      ) : (
                        <>
                          <PaperclipIcon />
                          <div className="attachment-info">
                            {isPreviewable(a) && a.status === STATUS_READY ? (
                              <button
                                type="button"
                                className="attachment-name attachment-name-preview"
                                title={`Preview ${a.fileName}`}
                                onClick={() => handlePreview(a)}
                              >
                                {a.fileName}
                              </button>
                            ) : (
                              <span className="attachment-name" title={a.fileName}>
                                {a.fileName}
                              </span>
                            )}
                            <span className="attachment-meta">
                              {formatFileSize(a.fileSize)}
                              {a.description ? ` · ${a.description}` : ''}
                              {a.status !== STATUS_READY ? ' · processing…' : ''}
                            </span>
                          </div>

                          <div className="attachment-actions">
                            <button
                              type="button"
                              className="icon-button"
                              title="Download"
                              aria-label={`Download ${a.fileName}`}
                              disabled={downloadingId === a.id || a.status !== STATUS_READY}
                              onClick={() => handleDownload(a)}
                            >
                              <DownloadIcon />
                            </button>

                            <button
                              type="button"
                              className="icon-button attachment-menu-trigger"
                              title="More actions"
                              aria-label={`More actions for ${a.fileName}`}
                              onClick={(e) => toggleMenu(e, a)}
                            >
                              <KebabIcon />
                            </button>
                          </div>
                        </>
                      )}
                    </li>
                  ))}
                </ul>
              )}
            </div>
          </div>
        </Modal>
      )}

      {openMenuId !== null &&
        menuAnchor &&
        createPortal(
          <div
            className="attachment-menu"
            ref={menuRef}
            style={{ top: menuAnchor.top, right: menuAnchor.right }}
          >
            <button
              type="button"
              onClick={() => startEdit(attachments.find((a) => a.id === openMenuId))}
            >
              Edit
            </button>
            <button
              type="button"
              className="danger"
              disabled={removingId === openMenuId}
              onClick={() => handleRemove(attachments.find((a) => a.id === openMenuId))}
            >
              {removingId === openMenuId ? 'Removing…' : 'Remove'}
            </button>
          </div>,
          document.body
        )}

      {preview && (
        <div className="attachment-lightbox-backdrop" onMouseDown={closePreview}>
          <div className="attachment-lightbox" onMouseDown={(e) => e.stopPropagation()}>
            <button
              type="button"
              className="attachment-lightbox-close"
              onClick={closePreview}
              aria-label="Close preview"
            >
              ×
            </button>
            {previewLoading ? (
              <p className="status-hint">Loading preview…</p>
            ) : previewError ? (
              <div className="form-message error">{previewError}</div>
            ) : (
              <img src={preview.url} alt={preview.attachment.fileName} />
            )}
          </div>
        </div>
      )}
    </>
  );
}
