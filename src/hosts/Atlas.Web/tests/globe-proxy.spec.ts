import { expect, test } from '@playwright/test';
import { createServer as createHttpServer } from 'node:http';
import { get as httpsGet } from 'node:https';
import { connect as connectHttp2, constants } from 'node:http2';
import { mkdtempSync, readFileSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { execFileSync, spawnSync } from 'node:child_process';
import { createServer as createViteServer, type ViteDevServer } from 'vite';
import { sessionApiProxy } from '../dev/localApi';

for (const protocol of ['HTTP/1.1', 'HTTP/2']) {
  test(`closing HTTPS ${protocol} globe streams releases upstream connections`, async () => {
    test.skip(
      !!spawnSync('openssl', ['version']).error,
      'OpenSSL is needed to create this test-only TLS certificate.',
    );
    const directory = mkdtempSync(join(tmpdir(), 'atlas-globe-proxy-'));
    const key = join(directory, 'key.pem'),
      cert = join(directory, 'cert.pem');
    execFileSync(
      'openssl',
      [
        'req',
        '-x509',
        '-newkey',
        'rsa:2048',
        '-nodes',
        '-keyout',
        key,
        '-out',
        cert,
        '-days',
        '1',
        '-subj',
        '/CN=localhost',
      ],
      { stdio: 'ignore' },
    );
    let active = 0;
    const upstream = createHttpServer((_request, response) => {
      active++;
      response.writeHead(200, { 'content-type': 'text/event-stream' });
      response.write('data: {}\n\n');
      const heartbeat = setInterval(() => response.write(': heartbeat\n\n'), 30);
      response.on('close', () => {
        active--;
        clearInterval(heartbeat);
      });
    });
    await new Promise<void>((resolve) => upstream.listen(0, '127.0.0.1', resolve));
    const port = (upstream.address() as { port: number }).port;
    const previous = process.env.ATLAS_STORYBOOK_API_URL;
    process.env.ATLAS_STORYBOOK_API_URL = `http://127.0.0.1:${port}`;
    let vite: ViteDevServer | undefined;
    let client: ReturnType<typeof connectHttp2> | undefined;
    try {
      vite = await createViteServer({
        configFile: false,
        appType: 'custom',
        cacheDir: join(directory, 'vite-cache'),
        optimizeDeps: { noDiscovery: true, include: [] },
        server: {
          host: '127.0.0.1',
          port: 0,
          https: { key: readFileSync(key), cert: readFileSync(cert) },
          proxy: { '/atlas-session-api': sessionApiProxy() },
        },
      });
      await vite.listen();
      const frontendPort = (vite.httpServer!.address() as { port: number }).port;
      const origin = `https://127.0.0.1:${frontendPort}`;
      if (protocol === 'HTTP/2') client = connectHttp2(origin, { rejectUnauthorized: false });
      for (let i = 0; i < 12; i++) {
        let close: () => void;
        if (client) {
          const stream = client.request({ ':path': '/atlas-session-api/globe/events' });
          await new Promise<void>((resolve, reject) => {
            stream.once('data', () => resolve());
            stream.once('error', reject);
          });
          close = () => stream.close(constants.NGHTTP2_CANCEL);
        } else {
          close = await new Promise<() => void>((resolve, reject) => {
            const request = httpsGet(
              origin + '/atlas-session-api/globe/events',
              { rejectUnauthorized: false },
              (response) => {
                response.once('data', () =>
                  resolve(() => {
                    response.destroy();
                    request.destroy();
                  }),
                );
              },
            );
            request.once('error', reject);
          });
        }
        expect(active).toBe(1);
        close();
        // Check actual upstream sockets, not just browser-side listeners or visitor totals.
        await expect.poll(() => active, { timeout: 2000 }).toBe(0);
      }
    } finally {
      client?.destroy();
      await vite?.close();
      upstream.closeAllConnections();
      await new Promise<void>((resolve) => upstream.close(() => resolve()));
      rmSync(directory, { recursive: true, force: true });
      if (previous === undefined) delete process.env.ATLAS_STORYBOOK_API_URL;
      else process.env.ATLAS_STORYBOOK_API_URL = previous;
    }
  });
}
