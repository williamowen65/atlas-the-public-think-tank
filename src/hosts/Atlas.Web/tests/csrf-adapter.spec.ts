import { expect, test } from '@playwright/test';
import { apiRequest, clearCsrfTokens } from '../src/lib/api/apiRequest';

// Deterministic transport tests, without SQL or browser-cookie claims.
const originalFetch = globalThis.fetch;
const originalWindow = globalThis.window;
let requests: { path: string; token: string | null }[];
let preparations: number;
let rejectWrite: boolean;

test.beforeEach(() => {
  globalThis.window = {} as Window & typeof globalThis;
  clearCsrfTokens();
  requests = [];
  preparations = 0;
  rejectWrite = false;
  globalThis.fetch = async (input, init) => {
    const path = String(input);
    requests.push({ path, token: new Headers(init?.headers).get('X-CSRF-TOKEN') });
    expect(init?.credentials).toBe('same-origin');
    if (path.endsWith('/auth/csrf')) return Response.json({ token: `token-${++preparations}` });
    return new Response(null, { status: rejectWrite ? 400 : 204 });
  };
});
test.afterEach(() => {
  clearCsrfTokens();
  globalThis.fetch = originalFetch;
  if (originalWindow === undefined) Reflect.deleteProperty(globalThis, 'window');
  else globalThis.window = originalWindow;
});

test('concurrent and sequential writes reuse preparation; public reads need no token', async () => {
  await apiRequest('/public', 'nodes');
  await Promise.all([
    apiRequest('/session', 'nodes/one/vote', 'PUT', { value: 5 }),
    apiRequest('/session', 'nodes/two/vote', 'PUT', { value: 6 }),
  ]);
  await apiRequest('/session', 'nodes/three/vote', 'DELETE');
  expect(preparations).toBe(1);
  expect(requests.filter((r) => r.token).map((r) => r.token)).toEqual(Array(3).fill('token-1'));
  await apiRequest('/other-session', 'nodes', 'POST', {});
  expect(preparations).toBe(2);
});

test('login and logout discard tokens before the next identity writes', async () => {
  await apiRequest('/session', 'auth/login', 'POST', {});
  await apiRequest('/session', 'nodes', 'POST', {});
  await apiRequest('/session', 'auth/logout', 'POST');
  await apiRequest('/session', 'auth/login', 'POST', {});
  expect(requests.filter((r) => r.token).map((r) => r.token)).toEqual([
    'token-1',
    'token-2',
    'token-2',
    'token-3',
  ]);
});

test('rejection clears cached token without replaying the write', async () => {
  rejectWrite = true;
  await expect(apiRequest('/session', 'nodes', 'POST', {})).rejects.toMatchObject({ status: 400 });
  expect(requests.filter((r) => r.path.endsWith('/nodes'))).toHaveLength(1);
  rejectWrite = false;
  await apiRequest('/session', 'nodes', 'POST', {});
  expect(preparations).toBe(2);
});

test('identity change during preparation prevents the waiting mutation', async () => {
  let release!: (response: Response) => void;
  globalThis.fetch = () =>
    new Promise<Response>((resolve) => {
      release = resolve;
    });
  const writing = apiRequest('/session', 'nodes', 'POST', {});
  clearCsrfTokens();
  release(Response.json({ token: 'old-identity' }));
  await expect(writing).rejects.toThrow('Your session changed');
});

test('one cancelled waiter does not cancel shared preparation', async () => {
  let release!: (response: Response) => void;
  const mock = globalThis.fetch;
  globalThis.fetch = (input, init) =>
    String(input).endsWith('/auth/csrf')
      ? new Promise<Response>((resolve) => {
          release = resolve;
        })
      : mock(input, init);
  const controller = new AbortController();
  const cancelled = apiRequest('/session', 'nodes', 'POST', {}, controller.signal);
  const surviving = apiRequest('/session', 'nodes', 'POST', {});
  controller.abort();
  await expect(cancelled).rejects.toMatchObject({ name: 'AbortError' });
  release(Response.json({ token: 'shared-token' }));
  await surviving;
  expect(requests).toEqual([{ path: '/session/nodes', token: 'shared-token' }]);
});

test('failed preparation can be tried by a later action', async () => {
  const mock = globalThis.fetch;
  globalThis.fetch = async () => {
    throw new TypeError('offline');
  };
  await expect(apiRequest('/session', 'nodes', 'POST', {})).rejects.toThrow('offline');
  globalThis.fetch = mock;
  await apiRequest('/session', 'nodes', 'POST', {});
  expect(preparations).toBe(1);
});

test('lost logout response still clears the cached authenticated token', async () => {
  await apiRequest('/session', 'nodes', 'POST', {});
  const mock = globalThis.fetch;
  globalThis.fetch = async (input, init) => {
    if (String(input).endsWith('/auth/logout')) throw new TypeError('lost response');
    return mock(input, init);
  };
  await expect(apiRequest('/session', 'auth/logout', 'POST')).rejects.toThrow('lost response');
  await apiRequest('/session', 'auth/login', 'POST', {});
  expect(preparations).toBe(2);
});

test('server calls never reuse a browser or another server request token', async () => {
  await apiRequest('/session', 'nodes', 'POST', {});
  Reflect.deleteProperty(globalThis, 'window');
  await apiRequest('/session', 'nodes', 'POST', {});
  await apiRequest('/session', 'nodes', 'POST', {});
  expect(preparations).toBe(3);
});

test('invalid token payload is not cached or sent in a write', async () => {
  const mock = globalThis.fetch;
  globalThis.fetch = async () => Response.json({ token: null });
  await expect(apiRequest('/session', 'nodes', 'POST', {})).rejects.toThrow('prepare');
  expect(requests).toHaveLength(0);
  globalThis.fetch = mock;
  await apiRequest('/session', 'nodes', 'POST', {});
  expect(preparations).toBe(1);
});
