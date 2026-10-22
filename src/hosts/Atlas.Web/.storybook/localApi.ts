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
      if (!/^\/atlas-preview-api\/participants(?:\?(?:[^#]*)|\/([0-9a-f-]{36})(?:\?.*)?|$)/i.test(request.url) && !/^\/atlas-preview-api\/nodes(?:\?|$)/.test(request.url)
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

/** Separate, narrowly scoped session bridge for local comment and node authoring. */
export const commentsPreview: Plugin = {
  name: 'atlas-comments-preview',
  configureServer(server) {
    server.middlewares.use((request, response, next) => {
      if (!request.url?.startsWith('/atlas-comments-api')) return next();
      const host = request.headers.host ?? '';
      const hostname = host.replace(/:\d+$/, '');
      if (!['localhost', '127.0.0.1', '[::1]'].includes(hostname)) {
        response.statusCode = 403; response.end('Local comments require a loopback host.'); return;
      }
      const path = request.url.split('?')[0];
      const guid = '[0-9a-f-]{36}';
      const method = request.method ?? 'GET';
      const readable = new RegExp(`^/atlas-comments-api/nodes/${guid}/votes$`, 'i').test(path) || /^\/atlas-comments-api\/auth\/(csrf|session)$/.test(path)
        || new RegExp(`^/atlas-comments-api/participants/${guid}import type { Plugin, ProxyOptions } from 'vite';

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
      if (!/^\/atlas-preview-api\/participants(?:\?(?:[^#]*)|\/([0-9a-f-]{36})(?:\?.*)?|$)/i.test(request.url) && !/^\/atlas-preview-api\/nodes(?:\?|$)/.test(request.url)
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

/** Separate, narrowly scoped session bridge for local comment and node authoring. */
export const commentsPreview: Plugin = {
  name: 'atlas-comments-preview',
  configureServer(server) {
    server.middlewares.use((request, response, next) => {
      if (!request.url?.startsWith('/atlas-comments-api')) return next();
      const host = request.headers.host ?? '';
      const hostname = host.replace(/:\d+$/, '');
      if (!['localhost', '127.0.0.1', '[::1]'].includes(hostname)) {
        response.statusCode = 403; response.end('Local comments require a loopback host.'); return;
      }
      const path = request.url.split('?')[0];
      const guid = '[0-9a-f-]{36}';
      const method = request.method ?? 'GET';
      const readable = new RegExp(`^/atlas-comments-api/nodes/${guid}/votes$`, 'i').test(path) || /^\/atlas-comments-api\/auth\/(csrf|session)$/.test(path)
        , 'i').test(path);
      const profileEdit = method === 'PUT' && path === '/atlas-comments-api/participants/me';
      const nodeVote = ['PUT', 'DELETE'].includes(method) && new RegExp(`^/atlas-comments-api/nodes/${guid}/vote$`, 'i').test(path);
      const writable = profileEdit || nodeVote || method === 'POST' && (['/atlas-comments-api/nodes', '/atlas-comments-api/catalog/node-types', '/atlas-comments-api/auth/register'].includes(path) || /^\/atlas-comments-api\/auth\/(login|logout)$/.test(path)
        || new RegExp(`^/atlas-comments-api/nodes/${guid}/comments$`, 'i').test(path)
        || new RegExp(`^/atlas-comments-api/comments/${guid}/replies$`, 'i').test(path))
        || ['PUT', 'DELETE'].includes(method) && new RegExp(`^/atlas-comments-api/comments/${guid}$`, 'i').test(path);
      if (!(method === 'GET' && readable) && !writable) {
        response.statusCode = 405; response.end('Unsupported comment operation.'); return;
      }
      // Reject cross-site writes before forwarding cookies. Backend CSRF remains mandatory.
      if (writable) {
        const origin = request.headers.origin;
        if (!origin || ![`http://${host}`, `https://${host}`].includes(origin)
          || request.headers['sec-fetch-site'] === 'cross-site') {
          response.statusCode = 403; response.end('Same-origin requests are required.'); return;
        }
      }
      next();
    });
  },
};

export function commentsApiProxy(): ProxyOptions {
  const options = localApiProxy();
  return {
    ...options,
    rewrite: path => path.replace(/^\/atlas-comments-api/, '/api'),
    configure(proxy) {
      proxy.on('proxyReq', request => {
        request.removeHeader('authorization');
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
