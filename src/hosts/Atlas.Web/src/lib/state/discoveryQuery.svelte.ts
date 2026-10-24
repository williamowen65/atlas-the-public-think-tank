/**
 * Shared reactive search state used by the header editor, badges and compact global results.
 * `value` holds the applied query; the shared parser derives result text and API filters.
 * `initialized` records whether browser restoration has already been attempted.
 * Updating this state does not fetch data or update the URL; SearchResults owns fetching.
 * The .svelte.ts extension allows Svelte runes such as $state outside a component.
 */
export const discoveryQuery = $state({ value: '', initialized: false });
/** Versioned localStorage key for the last applied query on this browser origin. */
export const discoveryQueryStorageKey = 'atlas.discovery.query.v1';
/**
 * Publish an applied query to reactive consumers and remember it in browser storage.
 * DiscoverySearch calls this after validating user input; this helper only trims text.
 * Empty text clears the saved query. If storage is blocked, in-memory search still works.
 * @param value Search text and optional filter tokens, such as `orcas type:Idea`.
 * @example setDiscoveryQuery('orcas type:Idea');
 */
export function setDiscoveryQuery(value: string) {
  discoveryQuery.value = value.trim();
  if (typeof window === 'undefined') return;
  try {
    if (discoveryQuery.value) localStorage.setItem(discoveryQueryStorageKey, discoveryQuery.value);
    else localStorage.removeItem(discoveryQueryStorageKey);
  } catch {
    /* Private browsing or blocked storage should not disable search. */
  }
}
/**
 * Initialize the shared query once in the browser; AtlasHeader calls this on mount, even while the editor is closed.
 * Precedence is URL `q`, then URL `search`, then the saved localStorage value.
 * An explicitly empty URL parameter clears the query rather than restoring saved text.
 * Uses setDiscoveryQuery so restored text is trimmed, published and saved consistently.
 * Does nothing outside the browser or after initialization; it does not watch URL changes.
 * @example restoreDiscoveryQuery(); // Restore when the search component mounts.
 */
export function restoreDiscoveryQuery() {
  if (discoveryQuery.initialized || typeof window === 'undefined') return;
  // Mark the attempt even if browser storage is unavailable.
  discoveryQuery.initialized = true;
  const url = new URL(window.location.href);
  const shared = url.searchParams.get('q') ?? url.searchParams.get('search');
  let stored = '';
  try {
    stored = localStorage.getItem(discoveryQueryStorageKey) ?? '';
  } catch {
    /* Storage optional. */
  }
  setDiscoveryQuery(shared ?? stored);
}
