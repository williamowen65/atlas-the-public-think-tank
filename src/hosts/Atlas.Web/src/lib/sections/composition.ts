import { publicUrl } from '../api/nodePage';
import { example, newId, type Draft } from './sectionLayouts';

export type Composition = Omit<Draft, 'title'> & { version: 1 };

export function readComposition(value?: string): Composition | undefined {
  try {
    const data = JSON.parse(value ?? 'null');
    if (data?.version === 1 && ['layouts', 'sections', 'blocks', 'assignments'].every((key) => Array.isArray(data[key])))
      return data;
  } catch { /* Legacy blocks remain readable. */ }
}

export function writeComposition(draft: Draft): string {
  const { layouts, sections, blocks, assignments } = draft;
  return JSON.stringify({ version: 1, layouts, sections, blocks, assignments });
}

export function fromMarkdown(texts: string[]): Draft {
  const draft = example('blank');
  texts = texts.length ? texts : [''];
  draft.sections = texts.map(() => ({ id: newId(), layoutId: 'full' }));
  draft.blocks = texts.map((text) => ({ id: newId(), kind: 'text', text }));
  draft.assignments = draft.blocks.map(({ id }, index) => ({ sectionId: draft.sections[index].id, slotId: 'left', blockId: id }));
  return draft;
}

export function compositionExcerpt(value: string): string {
  const composition = readComposition(value);
  if (!composition) return '';
  for (const block of composition.blocks) {
    if (block.kind === 'text' && block.text.trim()) return block.text;
    if ('photos' in block) {
      const photo = block.photos.find((item) => item.caption.trim() || item.alt.trim());
      if (photo) return photo.caption.trim() || photo.alt.trim();
    }
  }
  return '';
}

export function validateComposition(value: string): string | null {
  const composition = readComposition(value);
  if (!composition?.blocks.length) return 'Add at least one content block.';
  if (value.length > 512000) return 'This composition is too large.';
  for (const block of composition.blocks) {
    if (block.kind === 'link' && !publicUrl(block.url)) return 'Use a valid HTTP or HTTPS link.';
    if ('photos' in block) {
      if (!block.photos.length || block.photos.some((photo) => !publicUrl(photo.src)))
        return 'Enter an HTTP or HTTPS URL for each photo, or remove the unfinished photo.';
      if (block.photos.some((photo) => !photo.alt.trim()))
        return 'Enter alternative text for each photo.';
    }
  }
  return null;
}
