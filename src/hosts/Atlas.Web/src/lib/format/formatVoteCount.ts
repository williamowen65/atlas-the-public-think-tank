/**
 * Compact visual counts. Keep the full integer in accessible labels and tooltips.
 * Supported suffixes are K, M and B; billions are the largest unit.
 * @param count Community vote count supplied by the backend.
 * @returns Compact display text; the original count remains unchanged.
 * Truncate to one decimal so 999,999 stays 999.9K rather than implying a million.
 */
export function formatVoteCount(count: number): string {
  const units = [[1e9, 'B'], [1e6, 'M'], [1e3, 'K']] as const;
  for (const [scale, suffix] of units) {
    if (count >= scale) return `${Math.floor(count / scale * 10) / 10}${suffix}`;
  }
  return String(count);
}
