import { useCallback, useMemo, useState } from 'react';
import { AuthContext } from './useAuth';
import { login as loginRequest, logout as logoutRequest } from '../api/authApi';
import { getUserIdFromToken } from '../utils/jwt';

const STORAGE_KEY = 'uris.auth';

function readStoredSession() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
}

function writeStoredSession(session) {
  try {
    if (session) {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
    } else {
      localStorage.removeItem(STORAGE_KEY);
    }
  } catch {
    // localStorage unavailable (private browsing, etc.) - session just won't survive a refresh.
  }
}

export default function AuthProvider({ children }) {
  const [session, setSession] = useState(() => readStoredSession());

  const login = useCallback(async (username, password) => {
    const result = await loginRequest(username, password);
    const nextSession = {
      token: result.accessToken,
      refreshToken: result.refreshToken,
      username: result.username,
      role: result.role,
      userId: getUserIdFromToken(result.accessToken),
    };
    setSession(nextSession);
    writeStoredSession(nextSession);
    return nextSession;
  }, []);

  const logout = useCallback(() => {
    const refreshToken = session?.refreshToken;
    setSession(null);
    writeStoredSession(null);
    if (refreshToken) {
      // best-effort - the user is logged out locally regardless of whether this succeeds
      logoutRequest(refreshToken).catch(() => {});
    }
  }, [session]);

  const value = useMemo(
    () => ({
      token: session?.token ?? null,
      refreshToken: session?.refreshToken ?? null,
      username: session?.username ?? null,
      role: session?.role ?? null,
      userId: session?.userId ?? null,
      isAuthenticated: Boolean(session?.token),
      login,
      logout,
    }),
    [session, login, logout]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
