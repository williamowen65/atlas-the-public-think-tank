import { expect, test } from '@playwright/test';
import { sessionApiGuard } from '../dev/localApi';
import type { IncomingMessage, ServerResponse } from 'node:http';

test('HTTP/2 authority supports LAN reads and same-origin writes without allowing other hosts', () => {
  // Save the real process environment; restore it even if an assertion fails.
  const names = ['ATLAS_LAN_ORIGIN', 'ATLAS_LAN_CERT', 'ATLAS_LAN_KEY'] as const;
  const previous = names.map(name => process.env[name]);
  try {
    process.env.ATLAS_LAN_ORIGIN = 'https://192.168.50.55:5173';
    process.env.ATLAS_LAN_CERT = 'fixture-cert';
    process.env.ATLAS_LAN_KEY = 'fixture-key';
    // This variable is declared here, not imported. The mock use() below captures
    // the real request handler registered by the production plugin.
    // TypeScript's ! says it will be assigned before run() calls it.
    let middleware!: (request: IncomingMessage, response: ServerResponse, next: () => void) => void;
    // configureServer is Vite's plugin hook, implemented in dev/localApi.ts.
    // Vite normally invokes it with its server. Here we call it ourselves with
    // only the server properties the guard uses. The type assertion narrows Vite's
    // broader hook type; it does not create a function or change runtime behavior.
    const configure = sessionApiGuard.configureServer as (server: unknown) => void;
    // No server or TLS listener is started. https:{} models an enabled HTTPS server.
    // use(handler) records the handler instead of installing it in a live server.
    configure({
      config: { server: { https: {} } },
      middlewares: { use: (handler: typeof middleware) => { middleware = handler; } },
    });
    // Simulate one request. next() means the guard permits forwarding; end() means
    // rejection. This verifies the guard, not TLS, proxy forwarding or backend auth.
    const run = (headers: Record<string, string>, method = 'GET') => {
      let continued = false;
      const response = { statusCode: 200, setHeader() {}, end() {} };
      middleware({
        url: '/atlas-session-api/auth/' + (method === 'GET' ? 'session' : 'login'),
        method, headers,
      } as IncomingMessage, response as unknown as ServerResponse, () => { continued = true; });
      return { continued, status: response.statusCode };
    };
    // HTTP/2 uses :authority; HTTP/1.1 uses Host. Both must support the same policy.
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
