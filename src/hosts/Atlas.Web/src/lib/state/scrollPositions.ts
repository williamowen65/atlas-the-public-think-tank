export type ScrollAnchor = { kind: 'node' | 'comment'; id: string; offset: number; page?: number };
type ScrollPosition = { y: number; anchor?: ScrollAnchor };

// Keep the last position for a route in this tab without changing SvelteKit's history entries.
const positions = new Map<string, ScrollPosition>();

export function saveScrollPosition(url: URL, position: ScrollPosition) {
  positions.set(url.pathname + url.search, position);
}

export function readScrollPosition(url: URL) {
  return positions.get(url.pathname + url.search);
}
