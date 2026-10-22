/** Use real Storybook URLs, preserving any global style parameters in the address. */
export function openStory(storyId: string, nodeId?: string, typeId?: string) {
  const target = window.top ?? window;
  const url = new URL(target.location.href);
  if (target !== window || !url.pathname.endsWith('iframe.html')) {
    url.pathname = url.pathname.replace(/[^/]*$/, '');
    url.searchParams.set('path', `/story/${storyId}`);
    url.searchParams.delete('id'); url.searchParams.delete('viewMode');
  } else { url.searchParams.set('id', storyId); url.searchParams.set('viewMode', 'story'); }
  if (nodeId) url.searchParams.set('args', `nodeId:${nodeId}${typeId ? `;initialTypeId:${typeId}` : ''}`); else url.searchParams.delete('args');
  url.hash = typeId ? 'contributions' : '';
  target.location.assign(url.href);
}
export const openNodeStory = (nodeId: string, typeId?: string) => openStory('compositions-node-page--playground', nodeId, typeId);

/** Open the profile composition with its public participant ID. */
export function openProfileStory(participantId: string) {
  const target = window.top ?? window;
  const url = new URL(target.location.href);
  if (target !== window || !url.pathname.endsWith('iframe.html')) {
    url.pathname = url.pathname.replace(/[^/]*$/, '');
    url.searchParams.set('path', '/story/compositions-profile-page--playground');
    url.searchParams.delete('id');
    url.searchParams.delete('viewMode');
  } else {
    url.searchParams.set('id', 'compositions-profile-page--playground');
    url.searchParams.set('viewMode', 'story');
  }
  url.searchParams.set('args', 'participantId:' + encodeURIComponent(participantId));
  url.hash = '';
  target.location.assign(url.href);
}
