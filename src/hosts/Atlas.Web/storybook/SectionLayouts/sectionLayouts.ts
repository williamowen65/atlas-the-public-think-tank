export type Slot = { id: string; weight: number };
export type Layout = { id: string; name: string; slots: Slot[]; sectionOnly?: boolean };
export type ImageWrap = { textId: string; imageId: string; side: 'left' | 'right'; anchor: number };
export type Section = { id: string; layoutId: string; breakBefore?: boolean; rows?: (string | null)[]; wrap?: ImageWrap };
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
export type Photo = { id: string; src: string; alt: string; caption: string; captionPlacement?: 'overlay' | 'below'; frame?: PhotoFrame };
export type GalleryStyle = 'grid' | 'mosaic' | 'masonry' | 'slideshow';
export type Example = 'blank' | 'editorial' | 'photoEssay' | GalleryStyle;
export type Block =
  | { id: string; kind: 'text'; text: string; alignment?: TextAlignment }
  | { id: string; kind: 'image' | 'gallery'; photos: Photo[]; galleryStyle?: GalleryStyle; galleryWidth?: 'full' | 'centered' };
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
];

export const newId = () => crypto.randomUUID();
export const columns = (slots: Slot[]) => slots.map(({ weight }) => `minmax(0, ${weight}fr)`).join(' ');
export const blockAt = (draft: Draft, sectionId: string, slotId: string) => {
  const entry = draft.assignments.find((item) => item.sectionId === sectionId && item.slotId === slotId);
  return draft.blocks.find(({ id }) => id === entry?.blockId);
};

export function sectionRows(section: Section, layout: Layout) {
  return (section.rows ?? [null]).map((id) => ({
    id,
    slots: id === null ? layout.slots : [{ id, weight: 1 }],
  }));
}

export function rowWrap(draft: Draft, section: Section, slots: Slot[]) {
  const wrap = section.wrap;
  if (!wrap) return;
  const blocks = slots.map(({ id }) => blockAt(draft, section.id, id));
  const text = blocks.find((block) => block?.id === wrap.textId);
  const image = blocks.find((block) => block?.id === wrap.imageId);
  if (text?.kind === 'text' && image?.kind === 'image') return { ...wrap, text, image };
}

export function applyLayout(draft: Draft, sectionId: string, layoutId: string): Draft {
  const section = draft.sections.find(({ id }) => id === sectionId);
  const layout = draft.layouts.find(({ id }) => id === layoutId);
  if (!section || !layout || section.layoutId === layoutId) return draft;
  const previous = draft.layouts.find(({ id }) => id === section.layoutId)!;
  const slotBlocks = previous.slots.map(({ id }) => blockAt(draft, sectionId, id)?.id);
  const blocks = layout.slots.length < previous.slots.length
    ? slotBlocks.filter((id) => id !== undefined) : slotBlocks;
  const extra: Section[] = [];
  const assignments = draft.assignments.filter((entry) =>
    entry.sectionId !== sectionId || !previous.slots.some(({ id }) => id === entry.slotId));
  for (const [index, blockId] of blocks.entries()) {
    if (!blockId) continue;
    const slot = layout.slots[index];
    const vertical = draft.assignments.find((entry) => entry.blockId === blockId)?.vertical;
    if (slot) assignments.push({ sectionId, slotId: slot.id, blockId, vertical });
    else {
      const id = newId();
      extra.push({ id, layoutId: 'full' });
      assignments.push({ sectionId: id, slotId: 'left', blockId, vertical });
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
const intro = 'A public square can be a shortcut or a place people choose to spend time. Our square already has the foot traffic. What it needs is a reason to pause.\n\nStart with the places people already stop. A bench in the shade, a wide route through the middle, and a few movable planters could give the space a different feel without closing it for construction.';
const pilot = '### Start with a small pilot\n\nFor two weekends, try seating that can be moved and leave a clear path across the square. Ask people who use the space which spots feel comfortable and which routes are still hard to navigate.\n\nRecord what works before committing to permanent changes. Share the results with neighbors and local businesses.';
const next = '### What to measure\n\nCount how long people stay, where they sit, and whether they can cross the square easily. Listen to people who use mobility aids, parents with strollers, and anyone carrying groceries. The pilot should make their trips easier too.';
const ending = '### What happens after the pilot\n\nInvite neighbors to suggest what stays, moves, or goes. Compare the feedback with what volunteers observed, then publish a short summary so people can see how the next design responds to their suggestions.';
const text = (id: string, value: string): Block => ({ id, kind: 'text', text: value });

export function example(kind: Example): Draft {
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
  } else if (kind === 'grid' || kind === 'mosaic' || kind === 'masonry' || kind === 'slideshow') {
    draft.title = 'A greener public square';
    const galleryPhotos = structuredClone(photos);
    if (kind === 'mosaic' || kind === 'masonry') {
      galleryPhotos.push(
        photo('walkway', 'photo-1470071459604-3b5ec3a7fe05', 'Road through the hills'),
        photo('trees', 'photo-1441974231531-c6227db76b6e', 'Trees along a woodland path'),
      );
    }
    if (kind === 'mosaic') galleryPhotos[0].captionPlacement = 'overlay';
    if (kind === 'masonry') {
      const ratios = ['2 / 3', '1 / 1', '4 / 5', '1 / 1', '1 / 1', '1 / 1'] as const;
      galleryPhotos[0].caption = '';
      galleryPhotos.forEach((image, index) => { image.frame = { ratio: ratios[index] }; });
    }
    add('full', { id: 'reference-photos', kind: 'gallery', photos: galleryPhotos, galleryStyle: kind });
    add('full', text('pilot', pilot));
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
  const copy: Section = { ...section, id: newId(), wrap: undefined };
  const copiedIds = new Map<string, string>();
  const blocks = [...draft.blocks];
  const assignments = [...draft.assignments];
  for (const entry of draft.assignments.filter((item) => item.sectionId === sectionId)) {
    const block = draft.blocks.find(({ id }) => id === entry.blockId)!;
    const id = newId();
    copiedIds.set(block.id, id);
    blocks.push(block.kind === 'text' ? { ...block, id }
      : { ...block, id, photos: block.photos.map((photo) => ({ ...photo, id: newId(), frame: photo.frame && { ...photo.frame } })) });
    assignments.push({ ...entry, sectionId: copy.id, blockId: id });
  }
  if (section.wrap) {
    const textId = copiedIds.get(section.wrap.textId);
    const imageId = copiedIds.get(section.wrap.imageId);
    if (textId && imageId) copy.wrap = { ...section.wrap, textId, imageId };
  }
  return { ...draft, sections: draft.sections.flatMap((item) => item === section ? [item, copy] : [item]),
    blocks, assignments };
}
