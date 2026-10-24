import { readFileSync } from 'node:fs';
import adapter from '@sveltejs/adapter-static';
import { vitePreprocess } from '@sveltejs/vite-plugin-svelte';

const { version } = JSON.parse(readFileSync(new URL('./package.json', import.meta.url), 'utf8'));

export default {
  // Compiles <style lang="scss"> automatically in the app and Storybook.
  preprocess: vitePreprocess(),
  kit: {
    // Use the Atlas.Web package version for the app.html version marker.
    version: { name: version },
    // Atlas.Api remains the backend. Output is static files plus the SPA fallback.
    adapter: adapter({ pages: 'dist', assets: 'dist', fallback: 'index.html' }),
  },
};
