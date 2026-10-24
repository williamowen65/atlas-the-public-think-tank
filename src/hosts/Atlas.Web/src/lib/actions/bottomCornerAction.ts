/**
 * Svelte action that positions Read more at the description’s bottom-right corner
 * while reserving its footprint so text and the voting dial do not overlap it.
 *
 * Attach to the description element with use:bottomCornerAction. Svelte supplies
 * that mounted DOM element as the first argument; do not call this function in markup.
 * The containing article must have a header, a .voting-dial, and a .read-more
 * descendant inside the description. Matching card CSS consumes --corner-top.
 * @param description Mounted description element containing the Read more wrapper.
 * @returns A destroy hook that Svelte calls when the element is removed.
 * @example
 * // Import in the component script:
 * // import { bottomCornerAction } from '$lib/actions/bottomCornerAction';
 * // Attach in markup: <p class="description" use:bottomCornerAction>...</p>
 */
export function bottomCornerAction(description: HTMLElement) {
  const card = description.closest('article')!;
  const action = description.querySelector<HTMLElement>('.read-more')!;
  const dial = card.querySelector<HTMLElement>('.voting-dial')!;
  // Browser-issued animation-frame handle, not elapsed time. Zero means none is queued.
  let frame = 0;
  let previousWidth = 0;
  const align = () => {
    frame = 0;
    const width = description.getBoundingClientRect().width;
    // Release the old float height when wrapping changes, so the card can shrink too.
    if (width !== previousWidth) {
      action.style.setProperty('--corner-top', '0px');
      previousWidth = width;
    }
    const top = description.getBoundingClientRect().top;
    const buttonHeight = action.getBoundingClientRect().height;
    description.style.minHeight = `${Math.max(buttonHeight, dial.getBoundingClientRect().bottom - top)}px`;
    const available = description.getBoundingClientRect().height;
    action.style.setProperty('--corner-top', `${Math.max(0, available - buttonHeight)}px`);
  };
  // Combine resize notifications into one measurement before the next repaint.
  const schedule = () => {
    if (!frame) frame = requestAnimationFrame(align);
  };
  // Browser globals need no import. Watch the pieces that can change text wrapping/height.
  const observer = new ResizeObserver(schedule);
  observer.observe(description);
  observer.observe(action);
  observer.observe(dial);
  observer.observe(card.querySelector('header')!);
  schedule();
  // Action cleanup runs when Svelte removes the attached element, including route changes.
  return {
    destroy() {
      observer.disconnect();
      cancelAnimationFrame(frame);
    },
  };
}
