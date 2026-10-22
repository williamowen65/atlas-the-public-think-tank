import { sveltekit } from '@sveltejs/kit/vite';
import { defineConfig } from 'vite';
import { anonymousPreview, localApiProxy, commentsPreview, commentsApiProxy } from './.storybook/localApi';

export default defineConfig({
  plugins: [sveltekit(), anonymousPreview, commentsPreview],
  server: { proxy: { '/atlas-preview-api': localApiProxy(), '/atlas-comments-api': commentsApiProxy() } },
});
