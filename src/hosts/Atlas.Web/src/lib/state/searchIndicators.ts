/** Compact hints about the accepted query, not unsaved search/toolbox edits. */
export interface SearchIndicators {
  /** Whether ordinary search text is present outside recognized filter tokens. */
  hasText: boolean;
  /** Whether a recognized filter token requests a constraint or includes archived nodes. */
  hasFilters: boolean;
}

/**
 * Summarize the same token syntax used by discovery without fetching or changing it.
 * This is a visual hint, not validation or confirmation of backend matches. Inert
 * archived:false and unbounded ranges do not count as applied filters.
 */
export function searchIndicators(query: string): SearchIndicators {
  let hasText = false;
  let hasFilters = false;
  for (const part of query.match(/"[^"]*"|\S+/g) ?? []) {
    const match = /^(type|author|community|votes|average|from|through|archived):(.+)$/i.exec(part);
    if (!match) {
      if (part.replace(/"/g, '').trim()) hasText = true;
      continue;
    }
    const key = match[1].toLowerCase();
    const value = match[2].replace(/^"|"$/g, '');
    if (key === 'archived') hasFilters ||= value === 'true';
    else if (key === 'votes' || key === 'average') {
      const range = /^(\d+(?:\.\d+)?)?\.\.(\d+(?:\.\d+)?)?$/.exec(value);
      hasFilters ||= !!range && !!(range[1] || range[2]);
    } else hasFilters ||= !!value.trim();
  }
  return { hasText, hasFilters };
}
