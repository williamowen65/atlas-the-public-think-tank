export interface NodeType { id: string; name: string; pluralName?: string; autoPluralize?: boolean; description?: string; isArchived?: boolean }
export interface ContentBlock { id: string; kind: string; content: { markdown?: string; url?: string; altText?: string; caption?: string; title?: string; description?: string; pollId?: string; chartId?: string } }
export interface NodeDetail { id: string; title: string; shortDescription: string; requestedTypeIds: string[]; status: string; hidden?: boolean }
export interface Comment { id: string; authorId: string; body: string | null; status: string; createdAt: string; updatedAt?: string; parentId?: string | null }
export interface CommentPage { items: Comment[]; next: { afterAt: string; afterId: string } | null }
export async function readJson<T>(base: string, path: string, signal: AbortSignal): Promise<T> {
  const response = await fetch(`${base}/${path}`, { credentials: 'omit', signal });
  if (!response.ok) throw new Error(`Request failed (${response.status})`);
  return response.json();
}
export async function readTypes(base: string, signal: AbortSignal) {
  const types = await readJson<NodeType[]>(base, 'catalog/node-types', signal);
  return types.filter(type => !type.isArchived);
}
export function publicUrl(value?: string): string | undefined {
  if (!value) return;
  try { const url = new URL(value); return ['http:', 'https:'].includes(url.protocol) ? url.href : undefined; } catch { return; }
}
