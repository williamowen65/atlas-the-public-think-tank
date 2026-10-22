/** Shared client-side Discovery query. The URL is not the transport for local filtering. */
export const discoveryQuery = $state({ value: '', initialized: false });
export const discoveryQueryStorageKey = 'atlas.discovery.query.v1';
export function setDiscoveryQuery(value: string) {
  discoveryQuery.value = value.trim();
  if (typeof window === 'undefined') return;
  try {
    if (discoveryQuery.value) localStorage.setItem(discoveryQueryStorageKey, discoveryQuery.value);
    else localStorage.removeItem(discoveryQueryStorageKey);
  } catch { /* Private browsing or blocked storage should not disable search. */ }
}
export function restoreDiscoveryQuery() {
  if (discoveryQuery.initialized || typeof window === 'undefined') return;
  discoveryQuery.initialized = true;
  const url = new URL(window.location.href);
  const shared = url.searchParams.get('q') ?? url.searchParams.get('search');
  let stored = '';
  try { stored = localStorage.getItem(discoveryQueryStorageKey) ?? ''; } catch { /* Storage optional. */ }
  setDiscoveryQuery(shared ?? stored);
}
