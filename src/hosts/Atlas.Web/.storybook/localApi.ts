import type { Plugin, ProxyOptions } from 'vite';

/** Development-only public reader; never forwards Atlas sessions or accepts writes. */
export const anonymousPreview: Plugin = {
  name: 'atlas-anonymous-preview',
  configureServer(server) {
    server.middlewares.use((request, response, next) => {
      if (!request.url?.startsWith('/atlas-preview-api')) return next();
      if (request.method !== 'GET') {
        response.statusCode = 405;
        response.setHeader('Allow', 'GET');
        response.end('This preview only supports anonymous reads.');
        return;
      }
      if (!/^\/atlas-preview-api\/nodes(?:\?|$)/.test(request.url)
        && !/^\/atlas-preview-api\/catalog\/node-types(?:\?|$)/.test(request.url)
        && !/^\/atlas-preview-api\/nodes\/[0-9a-f-]{36}(?:\/(?:ancestors|reactions|description|comments))?(?:\?|$)/i.test(request.url)) {
        response.statusCode = 404;
        response.end('Only public node discovery is available in this preview.');
        return;
      }
      next();
    });
  },
};

export function localApiProxy(): ProxyOptions {
  const target = process.env.ATLAS_STORYBOOK_API_URL ?? 'https://localhost:7240';
  const url = new URL(target);
  if (!['localhost', '127.0.0.1', '[::1]'].includes(url.hostname) || !['http:', 'https:'].includes(url.protocol)) {
    throw new Error('ATLAS_STORYBOOK_API_URL must be a local HTTP(S) origin.');
  }
  if (url.username || url.password || url.pathname !== '/' || url.search || url.hash) {
    throw new Error('ATLAS_STORYBOOK_API_URL must contain only the local origin.');
  }
  return {
    target: url.origin,
    changeOrigin: true,
    // Accept the local ASP.NET development certificate; this proxy is dev-only.
    secure: false,
    rewrite: path => path.replace(/^\/atlas-preview-api/, '/api'),
    configure(proxy) {
      proxy.on('proxyReq', request => {
        request.removeHeader('cookie');
        request.removeHeader('authorization');
      });
      proxy.on('proxyRes', response => { delete response.headers['set-cookie']; });
    },
  };
}
