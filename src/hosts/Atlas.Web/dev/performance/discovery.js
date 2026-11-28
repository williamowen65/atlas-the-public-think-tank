import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter } from 'k6/metrics';

const failures = new Counter('discovery_failures');
const vus = Number(__ENV.ATLAS_VUS);
export const options = {
  scenarios: {
    discovery: { executor: 'constant-vus', vus, duration: vus === 1 ? '20s' : '75s', gracefulStop: '10s' },
  },
  summaryTrendStats: ['count', 'med', 'p(95)', 'max'],
};

export default function () {
  let page = 1;
  while (page <= 12) {
    const response = http.get(`${__ENV.ATLAS_URL}/atlas-public-api/nodes?Page=${page}`, {
      timeout: '10s',
      redirects: 0,
      headers: __ENV.ATLAS_HOST ? { Host: __ENV.ATLAS_HOST } : {},
      tags: { name: 'Discovery pagination' },
    });
    let body;
    if (response.status === 200) {
      try { body = response.json(); } catch { /* Invalid JSON is counted below. */ }
    }
    const valid = check(response, {
      'valid Discovery response': () => Array.isArray(body?.items) && typeof body.hasNextPage === 'boolean',
    });
    failures.add(valid ? 0 : 1);
    if (__VU === 1 && __ITER === 0 && page === 1) console.log('ATLAS_LOAD_STARTED');
    if (!valid || !body.hasNextPage) break;
    page += 1;
    sleep(2 + Math.random() * 4);
  }
  sleep(3 + Math.random() * 4);
}

export function handleSummary(data) {
  return { stdout: '\nATLAS_K6_SUMMARY=' + JSON.stringify(data.metrics) + '\n' };
}
