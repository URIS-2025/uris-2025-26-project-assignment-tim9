// Novcani iznosi stizu kao decimal iz backenda i u JSON-u su obicni brojevi.
// Prikaz je uvek na dve decimale, da se 400 ne prikaze kao "400" a 400.5 kao "400.5".
export function formatMoney(value) {
  const number = Number(value ?? 0);
  if (Number.isNaN(number)) return '-';
  return number.toLocaleString('sr-RS', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

export function formatDate(value) {
  if (!value) return '-';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '-';
  return date.toLocaleDateString('sr-RS', { day: '2-digit', month: '2-digit', year: 'numeric' });
}
