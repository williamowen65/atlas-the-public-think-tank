import type { Plugin, ProxyOptions } from 'vite';

/** Only Storybook development registers this bridge; no Atlas app/session requests use it. */
export const rankingLabGuard: Plugin = {
  name: 'atlas-ranking-lab-guard',
  configureServer(server) {
    server.middlewares.use((request, response, next) => {
      if (!request.url?.startsWith('/atlas-ranking-lab')) return next();
      const host = request.headers.host ?? '';
      const hostname = host.replace(/:\d+$/, '');
      if (!['localhost', '127.0.0.1', '[::1]'].includes(hostname)
        || request.headers.origin !== `http://${host}`
        || request.headers['sec-fetch-site'] === 'cross-site') {
        response.statusCode = 403; response.end('Local same-origin requests only.'); return;
      }
      if (request.method !== 'POST' || request.url !== '/atlas-ranking-lab/run') {
        response.statusCode = 405; response.end('Only scenario runs are supported.'); return;
      }
      next();
    });
  },
};
export function rankingLabProxy(): ProxyOptions {
  return {
    target: 'http://127.0.0.1:7251', changeOrigin: true,
    rewrite: path => path.replace(/^\/atlas-ranking-lab/, ''),
    configure(proxy) {
      proxy.on('proxyReq', request => {
        request.removeHeader('cookie'); request.removeHeader('authorization');
      });
    },
  };
}
