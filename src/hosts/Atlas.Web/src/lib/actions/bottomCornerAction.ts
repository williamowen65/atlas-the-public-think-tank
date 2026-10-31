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
  let observedDial: HTMLElement | null = null;
  // Browser-issued animation-frame handle, not elapsed time. Zero means none is queued.
  let frame = 0;
  let previousWidth = 0;
  let previousText = '';
  const align = () => {
    frame = 0;
    // Mobile places both actions in a separate row. Do not retain desktop's
    // measured minimum height while that layout is active.
    if (getComputedStyle(action).position === 'absolute') {
      description.style.minHeight = '0px';
      action.style.removeProperty('--corner-top');
      previousWidth = 0;
      return;
    }
    // The compact mobile control mounts/unmounts its dial. Resolve the current
    // element each time instead of measuring a detached dial after a resize.
    const dial = card.querySelector<HTMLElement>('.voting-dial');
    if (dial !== observedDial) {
      if (observedDial) observer.unobserve(observedDial);
      if (dial) observer.observe(dial);
      observedDial = dial;
      previousWidth = 0;
    }
    const width = description.getBoundingClientRect().width;
    // Release the old float height when wrapping changes, so the card can shrink too.
    const text = description.textContent ?? '';
    if (width !== previousWidth || text !== previousText) {
      description.style.minHeight = '0px';
      previousText = text;
      action.style.setProperty('--corner-top', '0px');
      previousWidth = width;
    }
    const top = description.getBoundingClientRect().top;
    const buttonHeight = action.getBoundingClientRect().height;
    description.style.minHeight = `${Math.max(buttonHeight, (dial?.getBoundingClientRect().bottom ?? top) - top, description.getBoundingClientRect().height)}px`;
    // Retain the height required by wrapped text before moving the float. Otherwise
    // a line beside the button can alternate between two heights on every resize.
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
  // A dial may not exist until the visitor expands mobile voting.
  observedDial = card.querySelector<HTMLElement>('.voting-dial');
  if (observedDial) observer.observe(observedDial);
  observer.observe(card.querySelector('header')!);
  const mutations = new MutationObserver(schedule);
  mutations.observe(card, { childList: true, characterData: true, subtree: true });
  window.addEventListener('resize', schedule);
  schedule();
  // Action cleanup runs when Svelte removes the attached element, including route changes.
  return {
    destroy() {
      window.removeEventListener('resize', schedule);
      observer.disconnect();
      mutations.disconnect();
      cancelAnimationFrame(frame);
    },
  };
}
