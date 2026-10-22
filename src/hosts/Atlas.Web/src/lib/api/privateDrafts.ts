import type { NodeDraft } from '../components/NodeForm.svelte';
import type { BreadcrumbItem } from '../components/Breadcrumb.svelte';

export interface SavedDraft { id: string; ownerId: string; updatedAt: string; data: NodeDraft; breadcrumb?: BreadcrumbItem[]; parentTitle?: string }
const prefix = 'atlas:private-node-drafts:v1:';
const key = (owner: string) => prefix + owner;
function valid(value: unknown): value is SavedDraft {
  if (!value || typeof value !== 'object') return false;
  const entry = value as Partial<SavedDraft>;
  return typeof entry.id === 'string' && typeof entry.ownerId === 'string'
    && typeof entry.updatedAt === 'string' && !!entry.data && typeof entry.data.title === 'string'
    && typeof entry.data.markdown === 'string';
}
/** Device-local drafts, partitioned by signed-in participant. Never sent to the public discovery API. */
export function listDrafts(owner: string): SavedDraft[] {
  try {
    const data = JSON.parse(localStorage.getItem(key(owner)) ?? '[]') as unknown;
    return Array.isArray(data) ? data.filter(valid).filter(x => x.ownerId === owner) : [];
  } catch { return []; }
}
export function putDraft(owner: string, draft: SavedDraft): void {
  const items = listDrafts(owner).filter(item => item.id !== draft.id);
  localStorage.setItem(key(owner), JSON.stringify([draft, ...items].slice(0, 50)));
}
export function deleteDraft(owner: string, id: string): void {
  localStorage.setItem(key(owner), JSON.stringify(listDrafts(owner).filter(item => item.id !== id)));
}
