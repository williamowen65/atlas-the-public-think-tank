/**
 * Compact elapsed time for experimental feed bylines; full dates remain on time
 * elements and node pages. Future timestamps use "in" rather than negative ages.
 * @param timestamp ISO creation date supplied by the API.
 * @param now Current epoch milliseconds, injectable for deterministic checks.
 * @returns Compact age such as "2h ago", "3d ago", or "in 2d".
 */
export function relativeDate(timestamp: string, now = Date.now()): string {
  const at = Date.parse(timestamp);
  if (!Number.isFinite(at)) return 'Unknown date';
  const seconds = Math.floor(Math.abs(now - at) / 1000);
  if (seconds < 60) return 'Just now';
  const units = [[31536000, 'y'], [2592000, 'mo'], [604800, 'w'], [86400, 'd'], [3600, 'h'], [60, 'm']] as const;
  const [size, label] = units.find(([size]) => seconds >= size)!;
  const value = Math.floor(seconds / size);
  return at > now ? `in ${value}${label}` : `${value}${label} ago`;
}
