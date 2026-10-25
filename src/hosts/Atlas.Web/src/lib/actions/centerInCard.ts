/**
 * Svelte action that horizontally centers the Read more wrapper against its whole
 * article while keeping the wrapper in the description’s vertical text flow.
 *
 * Attach to the .read-more wrapper with use:centerInCard. Svelte passes the mounted
 * element automatically. The matching card CSS consumes --card-center-offset;
 * this action measures the offset, but does not apply the transform itself.
 * @param element Mounted action wrapper; without a containing article, do nothing.
 * @returns A destroy hook releasing resize observation when Svelte removes the element.
 * @example
 * // Import in the component script:
 * // import { centerInCard } from '$lib/actions/centerInCard';
 * // Attach in markup: <span class="read-more" use:centerInCard>...</span>
 */
export function centerInCard(element: HTMLElement) {
  const card = element.closest<HTMLElement>('article');
  if (!card) return;
  // Compare the wrapper’s horizontal center with the containing card’s center.
  const align = () => {
    const outer = card.getBoundingClientRect();
    const flow = element.getBoundingClientRect();
    element.style.setProperty(
      '--card-center-offset',
      `${(outer.left + outer.right - flow.left - flow.right) / 2}px`,
    );
  };
  // Recalculate when either box changes size; ResizeObserver is a browser global.
  const observer = new ResizeObserver(align);
  observer.observe(card);
  observer.observe(element);
  align();
  // Svelte calls destroy when the attached element is removed.
  return { destroy: () => observer.disconnect() };
}
