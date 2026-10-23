import { sveltekit } from '@sveltejs/kit/vite';
import { defineConfig } from 'vite';
import { publicApiGuard, publicApiProxy, sessionApiGuard, sessionApiProxy } from './dev/localApi';

export default defineConfig({
  plugins: [sveltekit(), publicApiGuard, sessionApiGuard],
  server: { proxy: { '/atlas-public-api': publicApiProxy(), '/atlas-session-api': sessionApiProxy() } },
});
