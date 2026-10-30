/**
 * Compact visual counts. Keep the full integer in accessible labels and tooltips.
 * Truncate to one decimal so 999,999 stays 999.9K rather than implying a million.
 */
export function formatVoteCount(count: number): string {
  const units = [[1e12, 'T'], [1e9, 'B'], [1e6, 'M'], [1e3, 'K']] as const;
  for (const [scale, suffix] of units) {
    if (count >= scale) return `${Math.floor(count / scale * 10) / 10}${suffix}`;
  }
  return String(count);
}
