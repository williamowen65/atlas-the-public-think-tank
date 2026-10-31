import { existsSync } from 'node:fs';
import { loadEnvFile } from 'node:process';
import { fileURLToPath } from 'node:url';
import { spawn } from 'node:child_process';
import { lanEnvironment } from './lanEnvironment.mjs';

/**
 * Load machine-local settings before either host starts. Node preserves explicit
 * shell variables over env-file values. Vite still validates the private HTTPS
 * origin and reads certificate contents using the existing LAN guard/configuration.
 */
function start() {
  const localFile = fileURLToPath(new URL('../.env.lan.local', import.meta.url));
  if (existsSync(localFile)) loadEnvFile(localFile);
  const env = lanEnvironment();
  const cli = fileURLToPath(new URL('../node_modules/concurrently/dist/bin/concurrently.js', import.meta.url));
  const child = spawn(process.execPath, [
    cli, '--kill-others', '--names', 'API,Web',
    'dotnet run --project ../Atlas.Api --launch-profile https',
    'npm run dev -- --host 0.0.0.0',
  ], { cwd: fileURLToPath(new URL('..', import.meta.url)), env, stdio: 'inherit' });
  child.once('error', error => { console.error(error.message); process.exitCode = 1; });
  child.once('exit', code => { process.exitCode = code ?? 1; });
  process.once('SIGINT', () => child.kill('SIGINT'));
  process.once('SIGTERM', () => child.kill('SIGTERM'));
}
try { start(); }
catch (error) { console.error(error instanceof Error ? error.message : error); process.exitCode = 1; }
