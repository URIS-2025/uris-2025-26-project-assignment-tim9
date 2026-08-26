// Minimal JWT payload decoder - no verification (that's the backend's job).
// We only need the "sub" claim (the user's id) out of the access token
// issued by AuthService.
export function decodeJwt(token) {
  try {
    const payload = token.split('.')[1];
    const base64 = payload.replace(/-/g, '+').replace(/_/g, '/');
    const padded = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), '=');
    const json = decodeURIComponent(
      atob(padded)
        .split('')
        .map((c) => '%' + c.charCodeAt(0).toString(16).padStart(2, '0'))
        .join('')
    );
    return JSON.parse(json);
  } catch {
    return null;
  }
}

export function getUserIdFromToken(token) {
  const payload = decodeJwt(token);
  return payload?.sub ?? null;
}
