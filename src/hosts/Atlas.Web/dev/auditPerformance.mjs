import { spawn } from 'node:child_process';
import { createInterface } from 'node:readline';
import { createReadStream } from 'node:fs';
import { mkdir, readdir, writeFile } from 'node:fs/promises';
import { randomUUID } from 'node:crypto';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { lanTelemetryDirectory, loadLanSettings } from './lanEnvironment.mjs';
import { measureBrowser } from './performance/browser.mjs';

const image = 'grafana/k6:1.5.0';
const scriptDirectory = fileURLToPath(new URL('./performance/', import.meta.url));
const outputDirectory = fileURLToPath(new URL('../../../../logs/observability/performance/', import.meta.url));

function startLoad(origin, local, vus) {
  const target = new URL(origin);
  const host = local ? target.host : '';
  if (local) target.hostname = 'host.docker.internal';
  const args = ['run', '--rm'];
  if (local && process.platform === 'linux') args.push('--add-host', 'host.docker.internal:host-gateway');
  args.push(
    '--mount', `type=bind,source=${scriptDirectory},target=/audit,readonly`,
    '-e', `ATLAS_URL=${target.origin}`,
    '-e', `ATLAS_HOST=${host}`,
    '-e', `ATLAS_LOCAL_HTTPS=${local && target.protocol === 'https:'}`,
    '-e', `ATLAS_VUS=${vus}`,
    image, 'run', '--quiet', '/audit/discovery.js',
  );
  const child = spawn('docker', args, { stdio: ['ignore', 'pipe', 'pipe'] });
  const started = Promise.withResolvers();
  let metrics;
  let active = true;
  const streams = [child.stdout, child.stderr].map(input => createInterface({ input }));
  for (const stream of streams) {
    stream.on('line', line => {
      if (line.includes('ATLAS_LOAD_STARTED')) started.resolve();
      else if (line.startsWith('ATLAS_K6_SUMMARY=')) {
        try { metrics = JSON.parse(line.slice('ATLAS_K6_SUMMARY='.length)); }
        catch { console.error('Could not read the k6 summary.'); }
      } else if (line.trim()) console.log(line);
    });
  }
  const finished = new Promise((resolve, reject) => {
    child.once('error', reject);
    child.once('close', code => {
      active = false;
      if (code !== 0 || !metrics) reject(new Error(`k6 finished without results (exit ${code}).`));
      else resolve(metrics);
    });
  });
  // A Docker startup failure must also release a probe waiting for traffic to begin.
  finished.then(
    () => started.reject(new Error('k6 finished before reporting active traffic.')),
    started.reject,
  );
  return { started: started.promise, finished, isActive: () => active };
}

function summarize(values) {
  const sorted = values.filter(Number.isFinite).sort((a, b) => a - b);
  const percentile = fraction => sorted[Math.ceil(sorted.length * fraction) - 1];
  return { count: sorted.length, p50: percentile(0.5), p95: percentile(0.95) };
}

async function localCacheMetrics(start, end) {
  const startedAt = Date.parse(start);
  const endedAt = Date.parse(end);
  const directory = lanTelemetryDirectory();
  const files = await readdir(directory).catch(error => {
    if (error.code === 'ENOENT') return [];
    throw error;
  });
  const phases = Object.create(null);
  const lookups = Object.create(null);
  const requests = [];
  for (const file of files.filter(name => /^metrics-.*\.jsonl$/.test(name))) {
    const lines = createInterface({ input: createReadStream(join(directory, file)) });
    for await (const line of lines) {
      let record;
      try { record = JSON.parse(line); } catch { continue; }
      const observedAt = Date.parse(record?.observed_at_utc);
      if (!(observedAt >= startedAt && observedAt < endedAt)) continue;
      if (record.name === 'atlas.discovery.phase.duration') {
        const key = `${record.phase}/${record.outcome}`;
        (phases[key] ??= []).push(record.value);
      }
      if (record.name === 'atlas.discovery.cache.lookups') {
        const key = `${record.layer}/${record.outcome}`;
        lookups[key] = (lookups[key] ?? 0) + record.value;
      }
      if (record.name === 'atlas.discovery.cache.request') requests.push(record.total_ms);
    }
  }
  return {
    directory,
    phaseDurationMs: Object.fromEntries(Object.entries(phases).map(([phase, values]) => [phase, summarize(values)])),
    lookups,
    cacheRequestMs: summarize(requests),
  };
}

async function main() {
  loadLanSettings();
  const [target = 'http://localhost:5173', mode = 'local', ...extra] = process.argv.slice(2);
  const origin = new URL(target);
  const local = ['localhost', '127.0.0.1', '[::1]'].includes(origin.hostname);
  const localHttps = local && origin.protocol === 'https:';
  if (extra.length || origin.href !== origin.origin + '/' ||
    (mode === 'local' ? !local || (!localHttps && origin.protocol !== 'http:') :
      mode !== 'demo' || origin.protocol !== 'https:' || local)) {
    throw new Error('Usage: npm run audit:performance -- [http(s)://localhost:5173] or -- https://<Atlas demo host> demo');
  }

  const runId = randomUUID();
  const report = { runId, origin: origin.origin, stages: [] };
  const path = join(outputDirectory, `${runId}.json`);
  await mkdir(outputDirectory, { recursive: true });
  console.log('Report:', path);
  let failed = false;
  for (const vus of [0, 1, 10, 25]) {
    const start = new Date().toISOString();
    const browserRequired = vus !== 1;
    const load = vus ? startLoad(origin.origin, local, vus) : undefined;
    const browserProbe = async () => {
      await load?.started;
      const browser = await measureBrowser(origin.origin, localHttps);
      if (load && !load.isActive()) throw new Error('k6 traffic ended before the browser probe completed.');
      return browser;
    };
    const [browserResult, loadResult] = await Promise.allSettled([
      browserRequired ? browserProbe() : undefined, load?.finished,
    ]);
    const end = new Date().toISOString();
    const browser = browserResult.value;
    const metrics = loadResult.value;
    const stage = {
      vus, start, end, browser,
      browserWaitMs: browser && summarize(browser.pagination.map(sample => sample.visibleWaitMs)),
      api: metrics && {
        requests: metrics.http_reqs?.values,
        durationMs: metrics.http_req_duration?.values,
        failures: metrics.discovery_failures?.values.count,
      },
      errors: [browserResult, loadResult].filter(result => result.status === 'rejected').map(result => result.reason.message),
      localCache: local ? await localCacheMetrics(start, end) : undefined,
    };
    report.stages.push(stage);
    await writeFile(path, JSON.stringify(report, null, 2) + '\n');
    console.log(JSON.stringify(stage, null, 2));
    failed = stage.errors.length > 0 || (stage.api && (!stage.api.requests?.count || stage.api.failures > 0)) ||
      (browser && (!browser.pagination.length || browser.pagination.some(sample => sample.outcome !== 'complete')));
    if (failed) break;
  }
  if (failed) process.exitCode = 1;
}

main().catch(error => {
  console.error(error.message);
  process.exitCode = 1;
});
