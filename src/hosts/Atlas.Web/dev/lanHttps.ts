import { readFileSync } from 'node:fs';

/**
 * Read the developer's explicitly configured browser origin (scheme + host + port).
 * Called by lanServer during Vite startup and by the session guard during registration.
 * No origin means normal desktop mode; invalid configuration stops startup.
 *
 * env defaults to Node's shell environment. Tests pass a separate object so they can
 * check validation without changing the real environment or reading certificate files.
 * This is development configuration, never browser code or a production auth policy.
 */
export function lanOrigin(env: NodeJS.ProcessEnv = process.env): URL | undefined {
  if (!env.ATLAS_LAN_ORIGIN) return undefined;
  // URL parses and normalizes the origin, including its port; malformed input throws.
  const url = new URL(env.ATLAS_LAN_ORIGIN);
  // Limit the opt-in address to private IPv4 ranges: 10/8, 172.16/12, 192.168/16.
  // This limits configuration mistakes; a private address is NOT proof of identity.
  const parts = url.hostname.split('.').map(Number);
  const privateIp = parts.length === 4 && parts.every(p => Number.isInteger(p) && p >= 0 && p <= 255)
    && (parts[0] === 10 || (parts[0] === 192 && parts[1] === 168)
      || (parts[0] === 172 && parts[1] >= 16 && parts[1] <= 31));
  // Require HTTPS for Secure cookies and reject credentials, paths and query strings:
  // this setting describes one server origin, not an arbitrary destination URL.
  if (url.protocol !== 'https:' || !privateIp || url.username || url.password
    || url.pathname !== '/' || url.search || url.hash)
    throw new Error('ATLAS_LAN_ORIGIN must be an HTTPS private IPv4 origin, e.g. https://192.168.1.25:5173');
  // Both paths must be supplied. lanServer reads the actual files at startup.
  if (!env.ATLAS_LAN_CERT || !env.ATLAS_LAN_KEY)
    throw new Error('LAN mode requires ATLAS_LAN_CERT and ATLAS_LAN_KEY paths.');
  return url;
}

/**
 * Build the optional server settings spread into vite.config.ts's server object.
 * Returning {} leaves the ordinary Vite settings alone. In LAN mode, Vite reads
 * the server certificate/private key from this PC and terminates HTTPS itself.
 * The phone receives the public certificate during TLS, never the private key.
 * File-read failures stop startup instead of silently falling back to HTTP.
 */
export function lanServer() {
  const origin = lanOrigin();
  if (!origin) return {};
  return {
    // Listen on all IPv4 interfaces so another device can reach this PC.
    // Firewall/network controls determine who can connect; this is not localhost-only.
    host: '0.0.0.0',
    port: Number(origin.port || 443),
    // Do not silently choose another port: the guard requires the exact configured origin.
    strictPort: true,
    https: {
      cert: readFileSync(process.env.ATLAS_LAN_CERT!),
      key: readFileSync(process.env.ATLAS_LAN_KEY!),
    },
  };
}
