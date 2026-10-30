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
    /**
     * The production request handler, captured when the mock server registers it.
     * Declared here rather than imported; ! tells TypeScript it is assigned before use.
     */
    let middleware!: (request: IncomingMessage, response: ServerResponse, next: () => void) => void;
    /**
     * Reference to Vite's configureServer hook implemented by the production plugin.
     * The test invokes it with a minimal mock server. The assertion only narrows
     * the hook's TypeScript type; it does not create or replace its implementation.
     * @param server Mock Vite server exposing configuration and middleware registration.
     * @returns Nothing; registration supplies the handler captured above.
     */
    const configure = sessionApiGuard.configureServer as (server: unknown) => void;
    // No server or TLS listener is started. https:{} models an enabled HTTPS server.
    // use(handler) records the handler instead of installing it in a live server.
    configure({
      config: { server: { https: {} } },
      middlewares: { use: (handler: typeof middleware) => { middleware = handler; } },
    });
    /**
     * Simulate one request through the captured production guard.
     * next() records permission to forward; response.end() models rejection.
     * This checks middleware policy, not real TLS, proxy forwarding or backend auth.
     * @param headers HTTP/1.1 Host or HTTP/2 authority and optional browser-origin headers.
     * @param method HTTP method; defaults to GET for session reads.
     * @returns Whether the handler continued and the resulting HTTP status.
     */
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
