import { existsSync } from 'node:fs';
import { join } from 'node:path';

/**
 * Prepare only the LAN launcher's environment; ordinary dev/build commands stay unchanged.
 * Certificates remain outside the repository and explicit shell/file values win over defaults.
 * @param {NodeJS.ProcessEnv} env Environment populated from the shell and optional local env file.
 * @param {(path: string) => boolean} exists Injectable file check for deterministic tests.
 * @returns {NodeJS.ProcessEnv} Prepared child-process environment.
 * @throws {Error} If the origin or certificate files are missing.
 */
export function lanEnvironment(env = process.env, exists = existsSync) {
  const prepared = { ...env };
  if (!prepared.ATLAS_LAN_ORIGIN)
    throw new Error('Set ATLAS_LAN_ORIGIN in .env.lan.local (copy .env.lan.example first).');
  const directory = prepared.LOCALAPPDATA && join(prepared.LOCALAPPDATA, 'Atlas', 'dev-certs');
  prepared.ATLAS_LAN_CERT ||= directory && join(directory, 'atlas.pem');
  prepared.ATLAS_LAN_KEY ||= directory && join(directory, 'atlas-key.pem');
  for (const name of ['ATLAS_LAN_CERT', 'ATLAS_LAN_KEY']) {
    const path = prepared[name];
    if (!path || !exists(path))
      throw new Error(name + ' must point to an existing certificate/key file. See docs/LAN-DEVELOPMENT.md.');
  }
  return prepared;
}
