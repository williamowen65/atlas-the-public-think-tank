import type { Plugin, ProxyOptions } from 'vite';
import { lanOrigin } from './lanHttps';

// Shared by the app's Vite server and Storybook's Vite server during development.
// Guards decide whether requests may continue; ProxyOptions configure forwarding.
// Neither supplies production routing. Atlas.Api still enforces auth and CSRF.

/** Development-only public reader; never forwards Atlas sessions or accepts writes. */
export const publicApiGuard: Plugin = {
  name: 'atlas-public-api-guard',
  configureServer(server) {
    server.middlewares.use((request, response, next) => {
      // Not our path: let the session guard or another middleware handle it.
      if (!request.url?.startsWith('/atlas-public-api')) return next();
      if (request.method !== 'GET') {
        response.statusCode = 405;
        response.setHeader('Allow', 'GET');
        response.end('The public API only supports anonymous reads.');
        return;
      }
      if (!/^\/atlas-public-api\/communities\/node\/[0-9a-f-]{36}(?:\?|$)/i.test(request.url)
        && !/^\/atlas-public-api\/communities(?:\/([0-9a-f-]{36})(?:\/nodes)?|\/participant\/[0-9a-f-]{36})?(?:\?|$)/i.test(request.url)
        && !/^\/atlas-public-api\/participants(?:\?(?:[^#]*)|\/([0-9a-f-]{36})(?:\?.*)?|$)/i.test(request.url)
        && !/^\/atlas-public-api\/(?:nodes|search)(?:\?|$)/.test(request.url)
       
        && !/^\/atlas-public-api\/catalog\/node-types(?:\?|$)/.test(request.url)
       
        && !/^\/atlas-public-api\/nodes\/[0-9a-f-]{36}\/comments\/[0-9a-f-]{36}\/path(?:\?|$)/i.test(request.url)
        && !/^\/atlas-public-api\/nodes\/[0-9a-f-]{36}(?:\/(?:ancestors|parents|reactions|description|comments))?(?:\?|$)/i.test(request.url)) {
        response.statusCode = 404;
        response.end('Only allowlisted public reads are available through this API.');
        return;
      }
      // Allowed: continue through middleware to Vite’s matching proxy.
      next();
    });
  },
};

// Called at startup: returns forwarding settings, rather than sending a request.
export function publicApiProxy(): ProxyOptions {
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
    // The browser calls this dev server; Vite forwards to the API’s /api path.
    rewrite: path => path.replace(/^\/atlas-public-api/, '/api'),
    configure(proxy) {
      proxy.on('proxyReq', request => {
        request.removeHeader('cookie');
        request.removeHeader('authorization');
      });
      proxy.on('proxyRes', response => { delete response.headers['set-cookie']; });
    },
  };
}

/** Development-only session bridge for authentication and allowlisted reads and writes. */
export const sessionApiGuard: Plugin = {
  name: 'atlas-session-api-guard',
  configureServer(server) {
    // Capture startup configuration. Vite owns TLS termination; middleware socket
    // wrappers need not expose TLSSocket.encrypted consistently.
    const lan = lanOrigin();
    const httpsEnabled = !!server.config.server.https;
    server.middlewares.use((request, response, next) => {
      // Ignore unrelated paths; an unmatched path is not a failed rule check.
      if (!request.url?.startsWith('/atlas-session-api')) return next();
      // 1. Restrict this session-aware development bridge to loopback hosts.
      // HTTP/2 uses :authority in place of the HTTP/1.1 Host header.
      // Reuse the resolved authority below for exact-host and same-origin checks.
      const authority = request.headers.host ?? request.headers[':authority'];
      const host = typeof authority === 'string' ? authority : '';
      const hostname = host.replace(/:\d+$/, '');
      const lanRequest = !!lan && host === lan.host && httpsEnabled;
      if (!['localhost', '127.0.0.1', '[::1]'].includes(hostname) && !lanRequest) {
        response.statusCode = 403;
        response.setHeader('Content-Type', 'text/plain; charset=utf-8');
        response.end([
          'The session API requires a loopback host or the configured LAN HTTPS origin.',
          `Received Host: ${host || '(missing)'}`,
          `Configured LAN Host: ${lan?.host ?? '(LAN mode disabled)'}`,
          `Vite HTTPS enabled: ${httpsEnabled}`,
          `Request authority: ${request.headers[':authority'] ?? '(not supplied)'}`,
        ].join('\n'));
        return;
      }
      // 2. Match only the explicitly supported paths and HTTP methods below.
      const path = request.url.split('?')[0];
      const guid = '[0-9a-f-]{36}';
      const method = request.method ?? 'GET';
      const readable = ['/atlas-session-api/communities/mine', '/atlas-session-api/communities/creation-allowance'].includes(path)
        || new RegExp(`^/atlas-session-api/nodes/${guid}/(?:votes|reactions)$`, 'i').test(path)
        || new RegExp(`^/atlas-session-api/comments/${guid}/votes$`, 'i').test(path)
        || /^\/atlas-session-api\/auth\/(csrf|session)$/.test(path)
        || new RegExp(`^/atlas-session-api/participants/${guid}$`, 'i').test(path);

      // Permitted writes, grouped by feature. Login/register can be anonymous.
      const nodeEdit = method === 'PUT' && new RegExp(`^/atlas-session-api/nodes/${guid}/(?:title|short-description)$`, 'i').test(path);
      const blockEdit = method === 'PUT' && new RegExp(`^/atlas-session-api/nodes/${guid}/description/blocks/${guid}$`, 'i').test(path);
      const profileEdit = method === 'PUT' && path === '/atlas-session-api/participants/me';
      const commentVote = ['PUT', 'DELETE'].includes(method) && new RegExp(`^/atlas-session-api/comments/${guid}/vote$`, 'i').test(path);
      const nodeVote = ['PUT', 'DELETE'].includes(method) && new RegExp(`^/atlas-session-api/nodes/${guid}/vote$`, 'i').test(path);
      const reactionVote = method === 'PUT' && new RegExp(`^/atlas-session-api/nodes/${guid}/reactions/vote$`, 'i').test(path);
      const undoReaction = method === 'DELETE' && new RegExp(`^/atlas-session-api/reactions/${guid}/vote$`, 'i').test(path);
      const communityWrite = (method === 'POST' && (path === '/atlas-session-api/communities' || new RegExp(`^/atlas-session-api/communities/${guid}/(?:archive|restore)$`, 'i').test(path)))
        || (method === 'PUT' && new RegExp(`^/atlas-session-api/communities/${guid}/(?:membership|name|description|nodes/${guid})$`, 'i').test(path))
        || (method === 'DELETE' && new RegExp(`^/atlas-session-api/communities/${guid}/(?:membership|nodes/${guid})$`, 'i').test(path));

      // Combine feature writes with creation, authentication and comment operations.
      const writable = nodeEdit || blockEdit || communityWrite || profileEdit || commentVote || nodeVote || reactionVote || undoReaction
        || method === 'POST' && (['/atlas-session-api/nodes', '/atlas-session-api/catalog/node-types', '/atlas-session-api/auth/register'].includes(path) || /^\/atlas-session-api\/auth\/(login|logout)$/.test(path)
        || new RegExp(`^/atlas-session-api/nodes/${guid}/comments$`, 'i').test(path)
        || new RegExp(`^/atlas-session-api/comments/${guid}/replies$`, 'i').test(path))
        || ['PUT', 'DELETE'].includes(method) && new RegExp(`^/atlas-session-api/comments/${guid}$`, 'i').test(path);

      // A request in our scope must match either an allowed GET or a write.
      if (!(method === 'GET' && readable) && !writable) {
        response.statusCode = 405;
        response.end('Unsupported session API operation.');
        return;
      }
      // 3. Reject cross-site writes before forwarding cookies. Backend CSRF remains mandatory.
      if (writable) {
        const origin = request.headers.origin;
        if (!origin || ![`http://${host}`, `https://${host}`].includes(origin)
          || request.headers['sec-fetch-site'] === 'cross-site') {
          response.statusCode = 403;
        response.end('Same-origin requests are required.');
        return;
        }
      }
      // Allowed: next() continues the chain; Vite already has the proxy settings.
      next();
    });
  },
};

export function sessionApiProxy(): ProxyOptions {
  // Reuse local-target validation, but replace the public proxy’s cookie handling.
  const options = publicApiProxy();
  return {
    ...options,
    rewrite: path => path.replace(/^\/atlas-session-api/, '/api'),
    configure(proxy) {
      proxy.on('proxyReq', request => {
        request.removeHeader('authorization');
        // Forward only Atlas session/CSRF cookies, including chunked sessions.
        const cookies = String(request.getHeader('cookie') ?? '').split(';').map(cookie => cookie.trim())
          .filter(cookie => /^__Host-Atlas\.(?:Csrf|Session(?:C\d+)?)=/.test(cookie));
        if (cookies.length) request.setHeader('cookie', cookies.join('; ')); else request.removeHeader('cookie');
      });
      proxy.on('proxyRes', response => {
        // Preserve Secure/HttpOnly/SameSite and __Host semantics; never weaken API cookies.
        const cookies = response.headers['set-cookie']?.filter(cookie => /^__Host-Atlas\.(?:Csrf|Session(?:C\d+)?)=/.test(cookie));
        if (cookies?.length) response.headers['set-cookie'] = cookies; else delete response.headers['set-cookie'];
        response.headers['cache-control'] = 'no-store';
      });
    },
  };
}
