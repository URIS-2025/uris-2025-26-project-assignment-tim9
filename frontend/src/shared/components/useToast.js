import { createContext, useContext } from 'react';

export const ToastContext = createContext(null);

// showToast(message, type) - type is 'success' | 'error' (anything else renders neutral).
// The toast removes itself after a few seconds; see ToastProvider.
export function useToast() {
  const ctx = useContext(ToastContext);
  if (!ctx) throw new Error('useToast must be used within a ToastProvider');
  return ctx;
}
