import adapter from '@sveltejs/adapter-static';
import { vitePreprocess } from '@sveltejs/vite-plugin-svelte';

export default {
  // Compiles <style lang="scss"> automatically in the app and Storybook.
  preprocess: vitePreprocess(),
  kit: {
    // Atlas.Api remains the backend. Output is static files plus the SPA fallback.
    adapter: adapter({ pages: 'dist', assets: 'dist', fallback: 'index.html' }),
  },
};
