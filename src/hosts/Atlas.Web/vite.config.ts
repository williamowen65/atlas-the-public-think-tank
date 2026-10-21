import { sveltekit } from '@sveltejs/kit/vite';
import { defineConfig } from 'vite';
import { anonymousPreview, localApiProxy } from './.storybook/localApi';

export default defineConfig({
  plugins: [sveltekit(), anonymousPreview],
  server: { proxy: { '/atlas-preview-api': localApiProxy() } },
});
