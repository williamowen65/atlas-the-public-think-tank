import type { BreadcrumbPage } from '../components/Breadcrumb.svelte';
import type { NodeCardData } from '../components/NodeCard.svelte';

export interface DiscoveryFilters { parentId?: string; typeId?: string; authorId?: string; communityId?: string; minVotes?: number; maxVotes?: number; minAverage?: number; maxAverage?: number; from?: string; through?: string; archived?: boolean }
interface DiscoveryCard {
  childTypes?: { typeId: string; label: string; count: number }[];
  nodeId: string; title: string; typeName: string; authorName: string; authorId?: string | null; status: string;
  breadcrumbSegment?: { items: { nodeId: string; label: string }[]; continuationNodeId: string | null; hasAlternativeRoutes: boolean; cycleDetected: boolean };
  createdAt: string; description: string; shortDescription: string; breadcrumb: string; subNodes: string;
  reactions: string; communities: string; commentCount: number; isHidden: boolean;
}
interface DiscoveryItem { nodeId: string; voteCount: number; averageVote: number | null; card: DiscoveryCard | null }
export interface DiscoveryPage {
  items: DiscoveryItem[]; totalCount: number; page: number; pageSize: number; hasNextPage: boolean;
}

/** Anonymous read only. The caller supplies an API base and owns request cancellation. */
export async function readDiscovery(base: string, page: number, search: string, signal: AbortSignal, nodeId?: string, filters?: DiscoveryFilters): Promise<DiscoveryPage> {
  const query = new URLSearchParams({ Page: String(page) });
  if (nodeId) { query.set('NodeId', nodeId); query.set('IncludeArchived', 'true'); }
  if (filters?.parentId) query.set('ParentNodeId', filters.parentId);
  if (filters?.typeId) query.set('NodeTypeId', filters.typeId);
  if (filters?.authorId) query.set('AuthorParticipantId', filters.authorId);
  if (filters?.communityId) query.set('CommunityId', filters.communityId);
  if (filters?.minVotes !== undefined) query.set('MinimumVoteCount', String(filters.minVotes));
  if (filters?.maxVotes !== undefined) query.set('MaximumVoteCount', String(filters.maxVotes));
  if (filters?.minAverage !== undefined) query.set('MinimumAverageVote', String(filters.minAverage));
  if (filters?.maxAverage !== undefined) query.set('MaximumAverageVote', String(filters.maxAverage));
  if (filters?.from) query.set('CreatedFrom', filters.from);
  if (filters?.through) query.set('CreatedThrough', filters.through);
  if (filters?.archived) query.set('IncludeArchived', 'true');
  if (search.trim()) query.set('SearchText', search.trim());
  const response = await fetch(`${base}/nodes?${query}`, { method: 'GET', credentials: 'omit', signal });
  if (!response.ok) throw new Error(`Atlas API returned HTTP ${response.status}.`);
  const result = await response.json() as DiscoveryPage;
  if (!Array.isArray(result.items) || !Number.isInteger(result.page) || !Number.isInteger(result.totalCount)) {
    throw new Error('Atlas API returned an unexpected Discovery response.');
  }
  return result;
}

/** Discovery strings remain display text; do not invent structured links or reaction counts. */
export function discoveryCard(item: DiscoveryItem): NodeCardData | null {
  const card = item.card;
  if (!card || card.isHidden) return null;
  return {
    id: item.nodeId, title: card.title, typeName: card.typeName, authorName: card.authorName, authorId: card.authorId ?? undefined,
    status: card.status === 'Archived' ? 'Archived' : 'Active', createdAt: card.createdAt,
    description: card.shortDescription ?? '', communities: card.communities ? [card.communities] : [],
    breadcrumb: card.breadcrumbSegment ? [...card.breadcrumbSegment.items].reverse().map(item => ({ id: item.nodeId, title: item.label })) : card.breadcrumb ? [{ title: card.breadcrumb }] : [], related: [], requestedTypes: [], reactions: [],
    childTypes: card.childTypes ?? [],
    reactionSummary: card.reactions, subNodeSummary: card.subNodes, commentCount: card.commentCount,
  };
}

/** The existing endpoint returns nearest-first; presentation pages are oldest-first. */
export async function readOlderAncestors(id: string, signal: AbortSignal, base = '/atlas-preview-api'): Promise<BreadcrumbPage> {
  const response = await fetch(`${base}/nodes/${encodeURIComponent(id)}/ancestors`, { credentials: 'omit', signal });
  if (!response.ok) throw new Error(`Could not load older ancestors (HTTP ${response.status}).`);
  const page = await response.json() as { items: { id: string; title: string }[]; hasMore: boolean };
  if (!Array.isArray(page.items) || typeof page.hasMore !== 'boolean') throw new Error('Unexpected ancestor response.');
  return { items: [...page.items].reverse(), hasMore: page.hasMore };
}

/** Read individual public reaction totals rather than parsing the formatted summary. */
export async function readReactions(id: string, signal: AbortSignal, base = '/atlas-preview-api'): Promise<NodeCardData['reactions']> {
  const response = await fetch(`${base}/nodes/${encodeURIComponent(id)}/reactions`, { credentials: 'omit', signal });
  if (!response.ok) throw new Error('Reaction totals could not be loaded.');
  const choices = await response.json() as { associationId: string | null; text: string; emoji: string; totalVotes: number }[];
  if (!Array.isArray(choices)) throw new Error('Unexpected reaction response.');
  return choices.filter(choice => choice.associationId && choice.totalVotes > 0)
    .map(choice => ({ label: `${choice.emoji} ${choice.text}`.trim(), count: choice.totalVotes }));
}
