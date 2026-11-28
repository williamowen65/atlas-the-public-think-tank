import { existsSync, readdirSync, readFileSync, statSync } from 'node:fs';
import { join } from 'node:path';
import { lanTelemetryDirectory, loadLanSettings } from './lanEnvironment.mjs';

try {
  loadLanSettings();
  const directory = lanTelemetryDirectory();
  const latest = (existsSync(directory) ? readdirSync(directory) : [])
    .filter(name => /^metrics-.*\.jsonl$/.test(name))
    .map(name => {
      const path = join(directory, name);
      return { path, modified: statSync(path).mtimeMs };
    })
    .sort((a, b) => b.modified - a.modified || b.path.localeCompare(a.path))[0];

  const ms = value => Number.isFinite(value) ? Math.round(value * 10) / 10 : undefined;
  const rows = [];
  if (latest) {
    for (const line of readFileSync(latest.path, 'utf8').trimEnd().split('\n')) {
      // The API may be appending to the last line while this command reads it.
      let record;
      try { record = JSON.parse(line); }
      catch { continue; }
      if (record?.name !== 'atlas.discovery.cache.request') continue;
      rows.push({
        time: new Date(record.observed_at_utc).toLocaleTimeString(),
        request: record.scope,
        page: record.page,
        ranking: record.ranking_source,
        memory: record.memory_cards,
        redis: record.redis_cards,
        sql: record.sql_cards,
        missing: record.missing_cards,
        'total ms': ms(record.total_ms),
        'lock ms': ms(record.lock_ms),
        'Redis page ms': ms(record.redis_page_ms),
        'SQL start ms': ms(record.sql_start_ms),
        'versions ms': ms(record.revision_ms),
        'count ms': ms(record.count_ms),
        'rank ms': ms(record.rank_ms),
        'cards ms': ms(record.cards_ms),
        'publish ms': ms(record.commit_publish_ms),
      });
    }
  }

  if (rows.length) {
    console.log(latest.path);
    console.table(rows);
  } else {
    console.log('No Discovery cache summaries in', latest?.path ?? directory);
    console.log('Start npm run dev:lan:api and load or scroll the Discovery feed, then rerun this command.');
  }
} catch (error) {
  console.error(error.message);
  process.exitCode = 1;
}
