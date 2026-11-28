/** Start optional panel reads once their card enters the viewport. */
export function preloadWhenVisible(element: HTMLElement, preload: () => void) {
  if (typeof IntersectionObserver === 'undefined') {
    preload();
    return;
  }

  const observer = new IntersectionObserver(entries => {
    if (!entries.some(entry => entry.isIntersecting)) return;
    observer.disconnect();
    preload();
  });

  observer.observe(element);
  return { destroy: () => observer.disconnect() };
}
