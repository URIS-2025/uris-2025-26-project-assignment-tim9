import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { ToastContext } from './useToast';
import './Toast.css';

const VISIBLE_MS = 4000;
const FADE_MS = 250;

let nextId = 0;

function ToastItem({ toast, onDismiss }) {
  // Start hidden, then flip to shown on the next tick so the CSS transition
  // runs. The resting state in Toast.css is fully visible, so even if
  // transitions never fire the toast still shows and still auto-dismisses.
  const [shown, setShown] = useState(false);

  useEffect(() => {
    const id = setTimeout(() => setShown(true), 30);
    return () => clearTimeout(id);
  }, []);

  const state = toast.leaving ? 'leaving' : shown ? 'shown' : 'enter';

  return (
    <div className={`toast toast--${toast.type} toast--${state}`} role="status">
      <span className="toast__message">{toast.message}</span>
      <button
        type="button"
        className="toast__close"
        aria-label="Dismiss notification"
        onClick={() => onDismiss(toast.id)}
      >
        ×
      </button>
    </div>
  );
}

export default function ToastProvider({ children }) {
  const [toasts, setToasts] = useState([]);
  const timers = useRef(new Map());

  const remove = useCallback((id) => {
    setToasts((list) => list.filter((t) => t.id !== id));
    const pending = timers.current.get(id);
    if (pending) {
      clearTimeout(pending.hide);
      clearTimeout(pending.drop);
      timers.current.delete(id);
    }
  }, []);

  const dismiss = useCallback(
    (id) => {
      setToasts((list) => list.map((t) => (t.id === id ? { ...t, leaving: true } : t)));
      const pending = timers.current.get(id) || {};
      clearTimeout(pending.hide);
      pending.drop = setTimeout(() => remove(id), FADE_MS);
      timers.current.set(id, pending);
    },
    [remove],
  );

  const showToast = useCallback(
    (message, type = 'info') => {
      const id = ++nextId;
      setToasts((list) => [...list, { id, message, type, leaving: false }]);
      timers.current.set(id, { hide: setTimeout(() => dismiss(id), VISIBLE_MS) });
    },
    [dismiss],
  );

  useEffect(() => {
    const map = timers.current;
    return () => {
      map.forEach(({ hide, drop }) => {
        clearTimeout(hide);
        clearTimeout(drop);
      });
      map.clear();
    };
  }, []);

  const value = useMemo(() => ({ showToast }), [showToast]);

  return (
    <ToastContext.Provider value={value}>
      {children}
      <div className="toast-viewport" aria-live="polite" aria-atomic="false">
        {toasts.map((toast) => (
          <ToastItem key={toast.id} toast={toast} onDismiss={dismiss} />
        ))}
      </div>
    </ToastContext.Provider>
  );
}
