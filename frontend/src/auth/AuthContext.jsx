import { useCallback, useMemo, useState } from 'react';
import * as authApi from '../api/authApi';
import { getUserIdFromToken } from './jwt';
import { AuthContext } from './useAuth';

const STORAGE_KEY = 'timelog.auth';

function loadStoredSession() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
}

function storeSession(session) {
  if (session) {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
  } else {
    localStorage.removeItem(STORAGE_KEY);
  }
}

export function AuthProvider({ children }) {
  const [session, setSession] = useState(loadStoredSession);

  const login = useCallback(async (username, password) => {
    const result = await authApi.login(username, password);
    const userId = getUserIdFromToken(result.accessToken);
    if (!userId) {
      throw new Error('Login succeeded but the access token had no user id.');
    }
    const next = {
      token: result.accessToken,
      refreshToken: result.refreshToken,
      username: result.username,
      role: result.role,
      userId,
    };
    setSession(next);
    storeSession(next);
    return next;
  }, []);

  const logout = useCallback(() => {
    setSession(null);
    storeSession(null);
  }, []);

  const value = useMemo(
    () => ({
      isAuthenticated: !!session,
      token: session?.token ?? null,
      userId: session?.userId ?? null,
      username: session?.username ?? null,
      role: session?.role ?? null,
      login,
      logout,
    }),
    [session, login, logout]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
