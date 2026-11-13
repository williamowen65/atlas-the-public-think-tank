export type Slot = { id: string; weight: number; area?: string; continuations?: string[] };
export type Layout = { id: string; name: string; slots: Slot[]; sectionOnly?: boolean; tracks?: number[] };
export type Section = { id: string; layoutId: string; breakBefore?: boolean };
export type VerticalAlignment = 'start' | 'center' | 'end';
export type TextAlignment = 'left' | 'center' | 'right';
export type Assignment = { sectionId: string; slotId: string; blockId: string; vertical?: VerticalAlignment };
export type PhotoFrame = {
  width?: number;
  ratio?: 'natural' | '1 / 1' | '4 / 5' | '2 / 3' | '4 / 3';
  zoom?: number;
  x?: number;
  y?: number;
  corners?: 'square' | 'rounded';
};
export type Photo = { id: string; src: string; alt: string; caption: string; frame?: PhotoFrame };
export type Block =
  | { id: string; kind: 'text'; text: string; alignment?: TextAlignment }
  | { id: string; kind: 'image' | 'gallery'; photos: Photo[] };
export type Draft = {
  title: string;
  layouts: Layout[];
  sections: Section[];
  blocks: Block[];
  assignments: Assignment[];
};

export const presets: Layout[] = [
  { id: 'full', name: 'Full width', slots: [{ id: 'left', weight: 1 }] },
  { id: 'equal', name: 'Two equal columns', slots: [{ id: 'left', weight: 1 }, { id: 'right', weight: 1 }] },
  { id: 'wide-left', name: 'Wide left', slots: [{ id: 'left', weight: 2 }, { id: 'right', weight: 1 }] },
  { id: 'wide-right', name: 'Wide right', slots: [{ id: 'left', weight: 1 }, { id: 'right', weight: 2 }] },
  { id: 'split-right', name: 'Split right column', tracks: [1, 1], slots: [
    { id: 'left', weight: 1, area: '1 / 1 / 3 / 2' },
    { id: 'right', weight: 1, area: '1 / 2 / 2 / 3' },
    { id: 'lower-right', weight: 1, area: '2 / 2 / 3 / 3' },
  ] },
  { id: 'split-left', name: 'Split left column', tracks: [1, 1], slots: [
    { id: 'left', weight: 1, area: '1 / 1 / 2 / 2' },
    { id: 'right', weight: 1, area: '1 / 2 / 3 / 3' },
    { id: 'lower-left', weight: 1, area: '2 / 1 / 3 / 2' },
  ] },
  { id: 'merged-text', name: 'L-shaped text', tracks: [1, 1], slots: [
    { id: 'left', weight: 1, area: '1 / 1 / 2 / 2', continuations: ['2 / 1 / 3 / 3'] },
    { id: 'right', weight: 1, area: '1 / 2 / 2 / 3' },
  ] },
  { id: 'merged-text-top', name: 'Upside-down L text', tracks: [1, 1], slots: [
    { id: 'left', weight: 1, area: '1 / 1 / 2 / 3', continuations: ['2 / 1 / 3 / 2'] },
    { id: 'right', weight: 1, area: '2 / 2 / 3 / 3' },
  ] },
  { id: 'merged-text-middle', name: 'Text around middle-right slot', tracks: [1, 1], slots: [
    { id: 'left', weight: 1, area: '1 / 1 / 2 / 3', continuations: ['2 / 1 / 3 / 2', '3 / 1 / 4 / 3'] },
    { id: 'right', weight: 1, area: '2 / 2 / 3 / 3' },
  ] },
];

export const newId = () => crypto.randomUUID();
export const columns = (layout: Layout) => (layout.tracks ?? layout.slots.map(({ weight }) => weight)).map((weight) => `minmax(0, ${weight}fr)`).join(' ');
// An explicit paragraph boundary makes the continuation independent of viewport height.
export function splitText(text: string, count = 2): string[] {
  const paragraphs = text.split(/\n[ \t]*\n/);
  return Array.from({ length: count }, (_, index) => index === count - 1
    ? paragraphs.slice(index).join('\n\n') : paragraphs[index] ?? '');
}

export const blockAt = (draft: Draft, sectionId: string, slotId: string) => {
  const entry = draft.assignments.find((item) => item.sectionId === sectionId && item.slotId === slotId);
  return draft.blocks.find(({ id }) => id === entry?.blockId);
};

export function applyLayout(draft: Draft, sectionId: string, layoutId: string): Draft {
  const section = draft.sections.find(({ id }) => id === sectionId);
  const layout = draft.layouts.find(({ id }) => id === layoutId);
  if (!section || !layout || section.layoutId === layoutId) return draft;
  const previous = draft.layouts.find(({ id }) => id === section.layoutId)!;
  const extra: Section[] = [];
  const assignments = draft.assignments.filter((entry) => entry.sectionId !== sectionId);
  const occupied = previous.slots.flatMap((slot) =>
    draft.assignments.filter((entry) => entry.sectionId === sectionId && entry.slotId === slot.id));
  const available = layout.slots.filter((slot) => !occupied.some((entry) => entry.slotId === slot.id));
  for (const entry of occupied) {
    const slot = layout.slots.find(({ id }) => id === entry.slotId) ?? available.shift();
    if (slot) assignments.push({ ...entry, slotId: slot.id });
    else {
      const id = newId();
      extra.push({ id, layoutId: 'full' });
      assignments.push({ ...entry, sectionId: id, slotId: 'left' });
    }
  }
  return {
    ...draft,
    sections: draft.sections.flatMap((item) => item.id === sectionId
      ? [{ ...item, layoutId }, ...extra] : [item]),
    assignments,
  };
}

export function moveBlock(draft: Draft, blockId: string, sectionId: string, slotId: string): Draft {
  const source = draft.assignments.find((entry) => entry.blockId === blockId);
  if (!source) return draft;
  const target = blockAt(draft, sectionId, slotId);
  return {
    ...draft,
    assignments: draft.assignments.map((entry) => entry.blockId === blockId
      ? { ...entry, sectionId, slotId }
      : entry.blockId === target?.id ? { ...entry, sectionId: source.sectionId, slotId: source.slotId } : entry),
  };
}

const photo = (id: string, url: string, alt: string, caption = ''): Photo =>
  ({ id, src: `https://images.unsplash.com/${url}?auto=format&fit=max&w=1000&q=80`, alt, caption });
const photos = [
  photo('street', 'photo-1519501025264-65ba15a82390', 'Buildings along a city street', 'Make room to walk and gather.'),
  photo('architecture', 'photo-1518005020951-eccb494ad742', 'Curving buildings seen from below'),
  photo('greenery', 'photo-1441974231531-c6227db76b6e', 'Sunlight filtering through tall trees'),
  photo('park', 'photo-1470071459604-3b5ec3a7fe05', 'Green hills and trees in the morning mist'),
];
const intro = 'A public square can be a shortcut or a place people choose to spend time. Our square already has the foot traffic. What it needs is a reason to pause.\n\nStart with the places people already stop. A bench in the shade, a wide route through the middle, and a few movable planters could give the space a different feel without closing it for construction.\n\nA short trial gives everyone a chance to see what works before the design becomes permanent.';
const pilot = '### Start with a small pilot\n\nFor two weekends, try seating that can be moved and leave a clear path across the square. Ask people who use the space which spots feel comfortable and which routes are still hard to navigate.\n\nRecord what works before committing to permanent changes. Share the results with neighbors and local businesses.';
const next = '### What to measure\n\nCount how long people stay, where they sit, and whether they can cross the square easily. Listen to people who use mobility aids, parents with strollers, and anyone carrying groceries. The pilot should make their trips easier too.';
const ending = '### What happens after the pilot\n\nInvite neighbors to suggest what stays, moves, or goes. Compare the feedback with what volunteers observed, then publish a short summary so people can see how the next design responds to their suggestions.';
const text = (id: string, value: string): Block => ({ id, kind: 'text', text: value });

export function example(kind: 'blank' | 'editorial' | 'photoEssay'): Draft {
  const draft: Draft = { title: '', layouts: structuredClone(presets), sections: [], blocks: [], assignments: [] };
  const add = (layoutId: string, ...blocks: Block[]) => {
    const sectionId = `section-${draft.sections.length + 1}`;
    draft.sections.push({ id: sectionId, layoutId });
    for (const [index, block] of blocks.entries()) {
      draft.blocks.push(block);
      draft.assignments.push({ sectionId, slotId: index === 0 ? 'left' : 'right', blockId: block.id });
    }
  };
  if (kind === 'blank') {
    add('full', text('opening', ''));
  } else {
    draft.title = 'A greener public square';
    if (kind === 'editorial') {
      add('wide-left', text('opening', intro), { id: 'street-photo', kind: 'image', photos: photos.slice(0, 1) });
    } else {
      add('equal', { id: 'reference-photos', kind: 'gallery', photos }, text('opening', intro));
      add('full', text('pilot', pilot));
    }
    add('equal', text('measure', next), text('afterward', ending));
  }
  return draft;
}

export function duplicateSection(draft: Draft, sectionId: string): Draft {
  const section = draft.sections.find(({ id }) => id === sectionId);
  if (!section) return draft;
  const copy = { ...section, id: newId() };
  const blocks = [...draft.blocks];
  const assignments = [...draft.assignments];
  for (const entry of draft.assignments.filter((item) => item.sectionId === sectionId)) {
    const block = draft.blocks.find(({ id }) => id === entry.blockId)!;
    const id = newId();
    blocks.push(block.kind === 'text' ? { ...block, id }
      : { ...block, id, photos: block.photos.map((photo) => ({ ...photo, id: newId(), frame: photo.frame && { ...photo.frame } })) });
    assignments.push({ ...entry, sectionId: copy.id, blockId: id });
  }
  return { ...draft, sections: draft.sections.flatMap((item) => item === section ? [item, copy] : [item]),
    blocks, assignments };
}
