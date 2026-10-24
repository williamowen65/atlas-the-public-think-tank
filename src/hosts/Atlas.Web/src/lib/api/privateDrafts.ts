import type { NodeDraft } from '../components/NodeForm.svelte';
import type { BreadcrumbItem } from '../components/Breadcrumb.svelte';

/** Device-local draft partitioned by participant, retaining optional parent context. */
export interface SavedDraft {
  /** Stable identifier of this record. */
  id: string;

  /** Participant ID used to partition local drafts. */
  ownerId: string;

  /** Most recent update timestamp, when provided. */
  updatedAt: string;

  /** Editable NodeDraft values saved on this device. */
  data: NodeDraft;

  /** Structured ancestor items retained with a child draft. */
  breadcrumb?: BreadcrumbItem[];

  /** Parent title retained for a child draft. */
  parentTitle?: string;
}
// Versioned localStorage namespace; each participant gets a separate key.
const prefix = 'atlas:private-node-drafts:v1:';
const key = (owner: string) => prefix + owner;
/**
 * Check the minimum saved-draft shape before trusting localStorage JSON; not full form
 * validation.
 */
function valid(value: unknown): value is SavedDraft {
  if (!value || typeof value !== 'object') return false;
  const entry = value as Partial<SavedDraft>;
  return (
    typeof entry.id === 'string' &&
    typeof entry.ownerId === 'string' &&
    typeof entry.updatedAt === 'string' &&
    !!entry.data &&
    typeof entry.data.title === 'string' &&
    typeof entry.data.markdown === 'string'
  );
}
/**
 * Device-local drafts, partitioned by signed-in participant. Never sent to the public
 * discovery API.
 */
export function listDrafts(owner: string): SavedDraft[] {
  try {
    const data = JSON.parse(localStorage.getItem(key(owner)) ?? '[]') as unknown;
    return Array.isArray(data) ? data.filter(valid).filter((x) => x.ownerId === owner) : [];
  } catch {
    return [];
  }
}
/**
 * Insert or replace a draft by ID, keeping at most 50 drafts for this participant. Storage
 * failures propagate.
 */
export function putDraft(owner: string, draft: SavedDraft): void {
  const items = listDrafts(owner).filter((item) => item.id !== draft.id);
  localStorage.setItem(key(owner), JSON.stringify([draft, ...items].slice(0, 50)));
}
/** Remove a draft from this participant’s device-local list. Storage failures propagate. */
export function deleteDraft(owner: string, id: string): void {
  localStorage.setItem(
    key(owner),
    JSON.stringify(listDrafts(owner).filter((item) => item.id !== id)),
  );
}
