import { readFileSync } from 'node:fs';

/** Explicit private IPv4 origin only; never widen the bridge to arbitrary hosts. */
export function lanOrigin(env: NodeJS.ProcessEnv = process.env): URL | undefined {
  if (!env.ATLAS_LAN_ORIGIN) return undefined;
  const url = new URL(env.ATLAS_LAN_ORIGIN);
  const parts = url.hostname.split('.').map(Number);
  const privateIp = parts.length === 4 && parts.every(p => Number.isInteger(p) && p >= 0 && p <= 255)
    && (parts[0] === 10 || (parts[0] === 192 && parts[1] === 168)
      || (parts[0] === 172 && parts[1] >= 16 && parts[1] <= 31));
  if (url.protocol !== 'https:' || !privateIp || url.username || url.password
    || url.pathname !== '/' || url.search || url.hash)
    throw new Error('ATLAS_LAN_ORIGIN must be an HTTPS private IPv4 origin, e.g. https://192.168.1.25:5173');
  if (!env.ATLAS_LAN_CERT || !env.ATLAS_LAN_KEY)
    throw new Error('LAN mode requires ATLAS_LAN_CERT and ATLAS_LAN_KEY paths.');
  return url;
}

export function lanServer() {
  const origin = lanOrigin();
  if (!origin) return {};
  return {
    host: '0.0.0.0',
    port: Number(origin.port || 443),
    strictPort: true,
    https: {
      cert: readFileSync(process.env.ATLAS_LAN_CERT!),
      key: readFileSync(process.env.ATLAS_LAN_KEY!),
    },
  };
}
