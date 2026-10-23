import { sveltekit } from '@sveltejs/kit/vite';
import { defineConfig } from 'vite';
import { publicApiGuard, publicApiProxy, sessionApiGuard, sessionApiProxy } from './dev/localApi';

export default defineConfig({
  // Guards check their own paths and call next() for unrelated or allowed requests.
  plugins: [sveltekit(), publicApiGuard, sessionApiGuard],
  // Register forwarding separately: these settings do not send requests at startup.
  server: {
    proxy: {
      '/atlas-public-api': publicApiProxy(),
      '/atlas-session-api': sessionApiProxy(),
    },
  },
});
