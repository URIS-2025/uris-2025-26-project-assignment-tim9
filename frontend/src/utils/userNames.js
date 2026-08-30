import { useEffect, useMemo, useState } from 'react';
import { getUserById } from '../api/userApi';

// Module-level cache so the same userId is only fetched once across every
// component/render. Values are either a resolved name string or the in-flight
// Promise that resolves to one.
const cache = new Map();

export function shortId(id) {
  return id ? `${String(id).slice(0, 8)}…` : '';
}

export function fetchUserName(userId, token) {
  if (!userId) return Promise.resolve('');
  const hit = cache.get(userId);
  if (hit !== undefined) return Promise.resolve(hit);

  const pending = getUserById(userId, token)
    .then((user) => {
      const name = user?.name || user?.username || shortId(userId);
      cache.set(userId, name);
      return name;
    })
    .catch(() => {
      const fallback = shortId(userId);
      cache.set(userId, fallback);
      return fallback;
    });

  cache.set(userId, pending);
  return pending;
}

/**
 * Resolves a set of user ids to display names, caching across the app.
 * Returns a lookup function: name(userId) -> string (falls back to a short id).
 */
export function useUserNames(ids, token) {
  const [names, setNames] = useState({});

  const idKey = useMemo(
    () => [...new Set((ids || []).filter(Boolean))].sort().join(','),
    [ids],
  );

  useEffect(() => {
    if (!idKey) return undefined;
    let cancelled = false;
    const unique = idKey.split(',');

    Promise.all(unique.map((id) => fetchUserName(id, token).then((name) => [id, name]))).then(
      (entries) => {
        if (!cancelled) setNames((prev) => ({ ...prev, ...Object.fromEntries(entries) }));
      },
    );

    return () => {
      cancelled = true;
    };
  }, [idKey, token]);

  return (userId) => {
    if (!userId) return 'Unassigned';
    return names[userId] || shortId(userId);
  };
}
