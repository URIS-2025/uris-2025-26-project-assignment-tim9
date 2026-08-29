function normalize(value) {
  if (value instanceof Date) {
    const time = value.getTime();
    return Number.isNaN(time) ? null : time;
  }
  return value;
}

function isNil(value) {
  return value === null || value === undefined || value === '';
}

export function sortBy(list, getter, dir = 'asc') {
  const factor = dir === 'desc' ? -1 : 1;
  return [...list].sort((a, b) => {
    const va = normalize(getter(a));
    const vb = normalize(getter(b));

    if (isNil(va) && isNil(vb)) return 0;
    if (isNil(va)) return 1;
    if (isNil(vb)) return -1;

    if (typeof va === 'string' && typeof vb === 'string') {
      return factor * va.localeCompare(vb, undefined, { sensitivity: 'base', numeric: true });
    }
    if (va < vb) return -factor;
    if (va > vb) return factor;
    return 0;
  });
}
