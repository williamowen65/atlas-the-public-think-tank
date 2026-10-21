/** Use real Storybook URLs, preserving any global style parameters in the address. */
export function openStory(storyId: string, nodeId?: string) {
  const target = window.top ?? window;
  const url = new URL(target.location.href);
  if (target !== window || !url.pathname.endsWith('iframe.html')) {
    url.pathname = url.pathname.replace(/[^/]*$/, '');
    url.searchParams.set('path', `/story/${storyId}`);
    url.searchParams.delete('id'); url.searchParams.delete('viewMode');
  } else { url.searchParams.set('id', storyId); url.searchParams.set('viewMode', 'story'); }
  if (nodeId) url.searchParams.set('args', `nodeId:${nodeId}`); else url.searchParams.delete('args');
  target.location.assign(url.href);
}
export const openNodeStory = (nodeId: string) => openStory('compositions-node-page--playground', nodeId);
