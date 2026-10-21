import type { NodeCardData } from '../components/NodeCard.svelte';

interface DiscoveryCard {
  nodeId: string; title: string; typeName: string; authorName: string; status: string;
  createdAt: string; description: string; breadcrumb: string; subNodes: string;
  reactions: string; communities: string; commentCount: number; isHidden: boolean;
}
interface DiscoveryItem { nodeId: string; voteCount: number; averageVote: number | null; card: DiscoveryCard | null }
export interface DiscoveryPage {
  items: DiscoveryItem[]; totalCount: number; page: number; pageSize: number; hasNextPage: boolean;
}

/** Anonymous read only. The caller supplies an API base and owns request cancellation. */
export async function readDiscovery(base: string, page: number, search: string, signal: AbortSignal): Promise<DiscoveryPage> {
  const query = new URLSearchParams({ Page: String(page) });
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
    id: item.nodeId, title: card.title, typeName: card.typeName, authorName: card.authorName,
    status: card.status === 'Archived' ? 'Archived' : 'Active', createdAt: card.createdAt,
    description: card.description, communities: card.communities ? [card.communities] : [],
    breadcrumb: card.breadcrumb ? [card.breadcrumb] : [], related: [], requestedTypes: [], reactions: [],
    reactionSummary: card.reactions, subNodeSummary: card.subNodes, commentCount: card.commentCount,
  };
}
