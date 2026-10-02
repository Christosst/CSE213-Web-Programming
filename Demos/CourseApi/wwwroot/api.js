// Used by all three pages. HTTP errors and network failures reach each page's catch.
export async function api(url, options = {}) {
  const response = await fetch(url, {
    ...options,
    headers: { 'Content-Type': 'application/json' }
  });
  if (response.status === 204) return null;
  const data = await response.json().catch(() => null);
  if (!response.ok) {
    const message = data?.errors
      ? Object.values(data.errors).flat().join(' ')
      : data?.title ?? `HTTP ${response.status}`;
    throw new Error(message);
  }
  return data;
}
