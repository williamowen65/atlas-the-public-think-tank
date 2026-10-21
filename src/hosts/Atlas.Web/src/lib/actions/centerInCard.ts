/** Keep the action in its text flow vertically, but align it to the whole card horizontally. */
export function centerInCard(element: HTMLElement) {
  const card = element.closest<HTMLElement>('article');
  if (!card) return;
  const align = () => {
    const outer = card.getBoundingClientRect();
    const flow = element.getBoundingClientRect();
    element.style.setProperty('--card-center-offset', `${(outer.left + outer.right - flow.left - flow.right) / 2}px`);
  };
  const observer = new ResizeObserver(align);
  observer.observe(card);
  observer.observe(element);
  align();
  return { destroy: () => observer.disconnect() };
}
