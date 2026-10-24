/**
 * Storybook-only Svelte action adapting ordinary application links to story navigation.
 * Capture before component handlers so workbench destinations remain in the workbench.
 * Modified clicks and links without a story destination keep normal browser behavior.
 */
export function storyLinks(element: HTMLElement, navigate: (url: URL) => boolean) {
  const click = (event: MouseEvent) => {
    if (event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey)
      return;
    const link =
      event.target instanceof Element ? event.target.closest<HTMLAnchorElement>('a[href]') : null;
    if (!link || link.target || link.hasAttribute('download')) return;
    const url = new URL(link.href);
    if (url.origin !== window.location.origin || !navigate(url)) return;
    event.preventDefault();
    event.stopPropagation();
  };
  element.addEventListener('click', click, true);
  return { destroy: () => element.removeEventListener('click', click, true) };
}
