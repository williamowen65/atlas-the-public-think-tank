import { expect, test } from '@playwright/test';
import { sessionApiGuard } from '../dev/localApi';
import type { IncomingMessage, ServerResponse } from 'node:http';

test('HTTP/2 authority supports LAN reads and same-origin writes without allowing other hosts', () => {
  const names = ['ATLAS_LAN_ORIGIN', 'ATLAS_LAN_CERT', 'ATLAS_LAN_KEY'] as const;
  const previous = names.map(name => process.env[name]);
  try {
    process.env.ATLAS_LAN_ORIGIN = 'https://192.168.50.55:5173';
    process.env.ATLAS_LAN_CERT = 'fixture-cert';
    process.env.ATLAS_LAN_KEY = 'fixture-key';
    let middleware!: (request: IncomingMessage, response: ServerResponse, next: () => void) => void;
    // Exercise the real guard with the HTTP/2 headers observed on the user's device.
    const configure = sessionApiGuard.configureServer as (server: unknown) => void;
    configure({
      config: { server: { https: {} } },
      middlewares: { use: (handler: typeof middleware) => { middleware = handler; } },
    });
    const run = (headers: Record<string, string>, method = 'GET') => {
      let continued = false;
      const response = { statusCode: 200, setHeader() {}, end() {} };
      middleware({
        url: '/atlas-session-api/auth/' + (method === 'GET' ? 'session' : 'login'),
        method, headers,
      } as IncomingMessage, response as unknown as ServerResponse, () => { continued = true; });
      return { continued, status: response.statusCode };
    };
    expect(run({ ':authority': '192.168.50.55:5173' }).continued).toBe(true);
    expect(run({ host: '192.168.50.55:5173' }).continued).toBe(true);
    expect(run({ ':authority': '192.168.50.55:5173', origin: 'https://192.168.50.55:5173' }, 'POST').continued).toBe(true);
    expect(run({ ':authority': '192.168.50.55:5173', origin: 'https://other.example' }, 'POST').status).toBe(403);
    expect(run({ ':authority': '192.168.50.99:5173' }).status).toBe(403);
    expect(run({}).status).toBe(403);
  } finally {
    names.forEach((name, index) => {
      if (previous[index] === undefined) delete process.env[name];
      else process.env[name] = previous[index];
    });
  }
});
