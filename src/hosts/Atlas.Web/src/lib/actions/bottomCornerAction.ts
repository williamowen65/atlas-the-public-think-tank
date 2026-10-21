/** Reserve only the bottom-right action's footprint in the description's text flow. */
export function bottomCornerAction(description: HTMLElement) {
  const card = description.closest('article')!;
  const action = description.querySelector<HTMLElement>('.read-more')!;
  const dial = card.querySelector<HTMLElement>('.voting-dial')!;
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
  const schedule = () => { if (!frame) frame = requestAnimationFrame(align); };
  const observer = new ResizeObserver(schedule);
  observer.observe(description);
  observer.observe(action);
  observer.observe(dial);
  observer.observe(card.querySelector('header')!);
  schedule();
  return { destroy() { observer.disconnect(); cancelAnimationFrame(frame); } };
}
